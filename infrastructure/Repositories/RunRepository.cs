using Backend.infrastructure.dataModels;
using Npgsql;

namespace Backend.infrastructure.Repositories;

public class RunRepository
{
    private NpgsqlDataSource _dataSource;

    public RunRepository(NpgsqlDataSource datasource)
    {
        _dataSource = datasource;
    }

    public async Task<string> LogRunToDb(int userId, string runId, double dtoStartingLat, double dtoStartingLng,
        DateTime? dateTime)
    {
        string insertedRunId = string.Empty;
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Insert into corsa.runs
                await using (var cmd = new NpgsqlCommand(
                                 "INSERT INTO corsa.runs (user_id, runID, startOfRun) VALUES (@userId, @runId, @startOfRun) RETURNING runID",
                                 connection))
                {
                    cmd.Parameters.AddWithValue("userId", userId);
                    cmd.Parameters.AddWithValue("runId", runId);
                    cmd.Parameters.AddWithValue("startOfRun", dateTime!);
                    insertedRunId = (string)await cmd.ExecuteScalarAsync();
                }

                // Insert into corsa.maps
                await using (var cmd = new NpgsqlCommand(
                                 "INSERT INTO corsa.maps (mapID, lat, lng, time) VALUES (@mapId, @lat, @lng, @time)",
                                 connection))
                {
                    cmd.Parameters.AddWithValue("mapId", runId);
                    cmd.Parameters.AddWithValue("lat", dtoStartingLat);
                    cmd.Parameters.AddWithValue("lng", dtoStartingLng);
                    cmd.Parameters.AddWithValue("time", dateTime);
                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                Console.WriteLine("Transaction committed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                await transaction.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }

        return insertedRunId;
    }


    public async Task LogCoordinatesToDb(string dtoRunId, double dtoLat, double dtoLng, string formattedLoggingTime)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            await using var cmd =
                new NpgsqlCommand("INSERT INTO corsa.maps (mapID, lat, lng, time) VALUES (@mapId, @lat, @lng, @time)",
                    connection);
            cmd.Parameters.AddWithValue("mapId", dtoRunId);
            cmd.Parameters.AddWithValue("lat", dtoLat);
            cmd.Parameters.AddWithValue("lng", dtoLng);
            cmd.Parameters.AddWithValue("time", formattedLoggingTime);

            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("Coordinates logged successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }
    }

    public async Task<RunInfoWithMap> LogEndingOfRunToDb(string dtoRunId, double dtoEndingLat, double dtoEndingLng,
        string formattedEndingTime)
    {
        RunInfoWithMap runInfo = null;
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Update corsa.runs with endOfRun and calculate timeOfRun
                await using (var cmd = new NpgsqlCommand(
                                 "UPDATE corsa.runs SET endOfRun = @endOfRun, timeOfRun = @timeOfRun WHERE runID = @runId",
                                 connection))
                {
                    cmd.Parameters.AddWithValue("runId", dtoRunId);
                    cmd.Parameters.AddWithValue("endOfRun", formattedEndingTime);

                    // Calculate timeOfRun
                    var startTime = await GetStartTimeOfRun(connection, dtoRunId);
                    var endTime = DateTime.Parse(formattedEndingTime);
                    var timeOfRun = endTime - startTime;
                    cmd.Parameters.AddWithValue("timeOfRun", timeOfRun.ToString(@"hh\:mm\:ss"));

                    await cmd.ExecuteNonQueryAsync();
                }

                // Insert last coordinates into corsa.maps
                await using (var cmd = new NpgsqlCommand(
                                 "INSERT INTO corsa.maps (mapID, lat, lng, time) VALUES (@mapId, @lat, @lng, @time)",
                                 connection))
                {
                    cmd.Parameters.AddWithValue("mapId", dtoRunId);
                    cmd.Parameters.AddWithValue("lat", dtoEndingLat);
                    cmd.Parameters.AddWithValue("lng", dtoEndingLng);
                    cmd.Parameters.AddWithValue("time", formattedEndingTime);

                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                Console.WriteLine("Ending of run logged successfully.");
                
                // Query the database to retrieve the run info
                await using (var cmd = new NpgsqlCommand(
                                 "SELECT runID, startOfRun, endOfRun, timeOfRun, distance FROM corsa.runs WHERE runID = @runId",
                                 connection))
                {
                    cmd.Parameters.AddWithValue("runId", dtoRunId);

                    await using var reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        runInfo = new RunInfoWithMap
                        {
                            RunId = reader.GetString(0),
                            StartOfRun = reader.GetDateTime(1),
                            EndOfRun = reader.GetDateTime(2),
                            TimeOfRun = reader.GetString(3),
                            Distance = reader.GetDouble(4),
                            Coordinates = new List<Coordinates>()
                        };
                    }
                }

                // Query the database to retrieve the list of coordinates
                await using (var cmd = new NpgsqlCommand(
                                 "SELECT lat, lng FROM corsa.maps WHERE mapID = @mapId ORDER BY time",
                                 connection))
                {
                    cmd.Parameters.AddWithValue("mapId", dtoRunId);

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        runInfo.Coordinates.Add(new Coordinates
                        {
                            Latitude = reader.GetDouble(0),
                            Longitude = reader.GetDouble(1),
                            TimeStamp = reader.GetDateTime(2)
                        });
                    }
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                await transaction.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }

        return runInfo;
    }

    private async Task<DateTime> GetStartTimeOfRun(NpgsqlConnection conn, string runId)
    {
        // Query corsa.runs table to get the startOfRun time
        var queryString = "SELECT startOfRun FROM corsa.runs WHERE runID = @runId";
        await using var cmd = new NpgsqlCommand(queryString, conn);
        cmd.Parameters.AddWithValue("runId", runId);
        var startTime = await cmd.ExecuteScalarAsync();
        return Convert.ToDateTime(startTime);
    }

    public async Task<string> SaveRunToDb(string runId, double dtoUserId, DateTime RunDateTime,
        TimeSpan formattedRunTime, double dtoRunDistance)
    {
        string insertedRunId = string.Empty;
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Insert into corsa.runs
                await using (var cmd = new NpgsqlCommand(
                                 "INSERT INTO corsa.runs (runID, user_id, startOfRun, timeOfRun, distance) VALUES (@runId, @userId, @startOfRun, @timeOfRun, @distance) RETURNING runID",
                                 connection))
                {
                    cmd.Parameters.AddWithValue("runId", runId);
                    cmd.Parameters.AddWithValue("userId", dtoUserId);
                    cmd.Parameters.AddWithValue("startOfRun", RunDateTime);
                    cmd.Parameters.AddWithValue("timeOfRun", formattedRunTime);
                    cmd.Parameters.AddWithValue("distance", dtoRunDistance);

                    insertedRunId = (string)await cmd.ExecuteScalarAsync();
                }

                await transaction.CommitAsync();
                Console.WriteLine("Run saved successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                await transaction.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }

        return insertedRunId;
    }


    public async Task<string> DeleteRunFromDb(int dtoUserId, string dtoRunId)
    {
        string deletedRunId = string.Empty;
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                // Check if the run belongs to the user
                var isAuthorized = await IsRunBelongsToUser(connection, dtoUserId, dtoRunId);
                if (!isAuthorized)
                {
                    Console.WriteLine($"User {dtoUserId} is not authorized to delete run {dtoRunId}.");
                    return null;
                }

                // Delete run from corsa.runs
                await using (var cmd = new NpgsqlCommand("DELETE FROM corsa.runs WHERE runID = @runId RETURNING runID",
                                 connection))
                {
                    cmd.Parameters.AddWithValue("runId", dtoRunId);
                    deletedRunId = (string)await cmd.ExecuteScalarAsync();
                }

                // Commit the transaction
                await transaction.CommitAsync();
                Console.WriteLine($"Run {dtoRunId} deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occurred: " + ex.Message);
                await transaction.RollbackAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }

        return deletedRunId;
    }

    private async Task<bool> IsRunBelongsToUser(NpgsqlConnection conn, int userId, string runId)
    {
        // Check if the run belongs to the user
        var queryString = "SELECT COUNT(*) FROM corsa.runs WHERE runID = @runId AND user_id = @userId";
        await using var cmd = new NpgsqlCommand(queryString, conn);
        cmd.Parameters.AddWithValue("runId", runId);
        cmd.Parameters.AddWithValue("userId", userId);
        var count = (long)await cmd.ExecuteScalarAsync();
        return count > 0;
    }

    public async Task<List<RunInfo>> GetAllRunsForUser(int dtoUserId)
    {
        var runs = new List<RunInfo>();
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            // Query all runs for the user from corsa.runs
            var queryString =
                "SELECT runID, startOfRun, endOfRun, timeOfRun, distance FROM corsa.runs WHERE user_id = @userId";
            await using var cmd = new NpgsqlCommand(queryString, connection);
            cmd.Parameters.AddWithValue("userId", dtoUserId);

            // Execute the query and read the results
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var runId = reader["runID"].ToString();
                var startOfRun = reader.GetDateTime(reader.GetOrdinal("startOfRun"));
                var endOfRun = reader.IsDBNull(reader.GetOrdinal("endOfRun"))
                    ? null
                    : (DateTime?)reader.GetDateTime(reader.GetOrdinal("endOfRun"));
                var timeOfRun = reader["timeOfRun"].ToString();
                var distance = Convert.ToDouble(reader["distance"]);

                runs.Add(new RunInfo
                {
                    RunId = runId,
                    StartOfRun = startOfRun,
                    EndOfRun = endOfRun,
                    TimeOfRun = timeOfRun,
                    Distance = distance
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }

        return runs;
    }

    public async Task<List<ProgressInfo>> GetProgressOfRunsForUser(int dtoUserId)
    {
        var progressList = new List<ProgressInfo>();
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            // Query the database to retrieve progress info for all runs of the user
            var queryString = "SELECT runID, timeOfRun, distance FROM corsa.runs WHERE user_id = @userId";
            await using var cmd = new NpgsqlCommand(queryString, connection);
            cmd.Parameters.AddWithValue("userId", dtoUserId);

            // Execute the query and read the results
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var runId = reader["runID"].ToString();
                var timeOfRun = reader["timeOfRun"].ToString();
                var distance = Convert.ToDouble(reader["distance"]);

                progressList.Add(new ProgressInfo
                {
                    RunId = runId,
                    TimeOfRun = timeOfRun,
                    Distance = distance
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }

        return progressList;
    }
}