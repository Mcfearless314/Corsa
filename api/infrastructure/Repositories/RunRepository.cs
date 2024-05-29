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
        DateTime dateTime)
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
                    cmd.Parameters.AddWithValue("startOfRun", dateTime);
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


    public async Task LogCoordinatesToDb(string dtoRunId, double dtoLat, double dtoLng, DateTime formattedLoggingTime)
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
        DateTime dtoRunEndTime)
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
                    cmd.Parameters.AddWithValue("endOfRun", dtoRunEndTime);

                    // Calculate timeOfRun
                    var startTime = await GetStartTimeOfRun(connection, dtoRunId);
                    var timeOfRun = dtoRunEndTime - startTime;
                    cmd.Parameters.AddWithValue("timeOfRun", timeOfRun);

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
                    cmd.Parameters.AddWithValue("time", dtoRunEndTime);

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
                            TimeOfRun = reader.GetTimeSpan(3).ToString(),
                            Distance = reader.IsDBNull(4) ? null : reader.GetDouble(4),
                            gpsCordsList = new List<Cords>()
                        };
                    }
                }

                // Query the database to retrieve the list of coordinates
                await using (var cmd = new NpgsqlCommand(
                                 "SELECT lat, lng, time FROM corsa.maps WHERE mapID = @mapId ORDER BY time",
                                 connection))
                {
                    cmd.Parameters.AddWithValue("mapId", dtoRunId);

                    await using var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        runInfo.gpsCordsList.Add(new Cords
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
               runs.Add(new RunInfo
                {
                    RunId = reader.GetString(0),
                    StartOfRun = reader.GetDateTime(1),
                    EndOfRun = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                    TimeOfRun = reader.IsDBNull(3) ? "00:00" : reader.GetTimeSpan(3).ToString(),
                    Distance = reader.IsDBNull(4) ? 0.0 : reader.GetDouble(4),
                    
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
                progressList.Add(new ProgressInfo
                {
                    RunId = reader.GetString(0),
                    TimeOfRun = reader.GetTimeSpan(1).ToString(),
                    Distance = reader.IsDBNull(2) ? 0.0 : reader.GetDouble(4),
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }

        return progressList;
    }

    public async Task<RunInfoWithMap> GetFullInfoOfRun(int dtoUserId, string dtoRunId)
    {
        RunInfoWithMap runInfo = null;
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            // Query the database to retrieve the run info
            await using (var cmd = new NpgsqlCommand(
                             "SELECT runID, startOfRun, endOfRun, timeOfRun, distance FROM corsa.runs WHERE runID = @runId AND user_id = @userId",
                             connection))
            {
                cmd.Parameters.AddWithValue("runId", dtoRunId);
                cmd.Parameters.AddWithValue("userId", dtoUserId);

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    runInfo = new RunInfoWithMap
                    {
                        RunId = reader.GetString(0),
                        StartOfRun = reader.GetDateTime(1),
                        EndOfRun = reader.IsDBNull(4) ? null : reader.GetDateTime(2),
                        TimeOfRun = reader.GetTimeSpan(3).ToString(),
                        Distance = reader.IsDBNull(4) ? null : reader.GetDouble(4),
                        gpsCordsList = new List<Cords>()
                    };
                }
            }

            // Query the database to retrieve the list of coordinates
            await using (var cmd = new NpgsqlCommand(
                             "SELECT lat, lng, time FROM corsa.maps WHERE mapID = @mapId ORDER BY time",
                             connection))
            {
                cmd.Parameters.AddWithValue("mapId", dtoRunId);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    runInfo.gpsCordsList.Add(new Cords
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
        }

        return runInfo;
    }

    public async Task UpdateDistanceOfRun(string dtoRunId, double totalDistance)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand(
                "UPDATE corsa.runs SET distance = @distance WHERE runID = @runId", connection);
            cmd.Parameters.AddWithValue("runId", dtoRunId);
            cmd.Parameters.AddWithValue("distance", totalDistance);

            await cmd.ExecuteNonQueryAsync();
            Console.WriteLine("Distance updated successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error occurred: " + ex.Message);
        }
    }
}