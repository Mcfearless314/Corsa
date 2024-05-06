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

    public async Task<string> LogRunToDb(string runId, double dtoStartingLat, double dtoStartingLng,
        string? formattedDateTime)
    {
        string insertedRunId = string.Empty;
        try
        {
            var connString = Environment.GetEnvironmentVariable("DBConnectionString");
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            await using var transaction = await conn.BeginTransactionAsync();
            try
            {
                // Insert into Corsa.runs
                await using (var cmd = new NpgsqlCommand(
                                 "INSERT INTO Corsa.runs (runID, startOfRun) VALUES (@runId, @startOfRun) RETURNING runID",
                                 conn))
                {
                    cmd.Parameters.AddWithValue("runId", runId);
                    cmd.Parameters.AddWithValue("startOfRun", formattedDateTime);
                    insertedRunId = (string)await cmd.ExecuteScalarAsync();
                }

                // Insert into Corsa.maps
                await using (var cmd = new NpgsqlCommand(
                                 "INSERT INTO Corsa.maps (mapID, lat, lng, time) VALUES (@mapId, @lat, @lng, @time)",
                                 conn))
                {
                    cmd.Parameters.AddWithValue("mapId", runId);
                    cmd.Parameters.AddWithValue("lat", dtoStartingLat);
                    cmd.Parameters.AddWithValue("lng", dtoStartingLng);
                    cmd.Parameters.AddWithValue("time", formattedDateTime);
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
            var connString = Environment.GetEnvironmentVariable("DBConnectionString");
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            await using var cmd =
                new NpgsqlCommand("INSERT INTO Corsa.maps (mapID, lat, lng, time) VALUES (@mapId, @lat, @lng, @time)",
                    conn);
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

    public async Task LogEndingOfRunToDb(string dtoRunId, double dtoEndingLat, double dtoEndingLng,
        string formattedEndingTime)
    {
        try
        {
            var connString = Environment.GetEnvironmentVariable("DBConnectionString");
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            await using var transaction = await conn.BeginTransactionAsync();
            try
            {
                // Update Corsa.runs with endOfRun and calculate timeOfRun
                await using (var cmd = new NpgsqlCommand(
                                 "UPDATE Corsa.runs SET endOfRun = @endOfRun, timeOfRun = @timeOfRun WHERE runID = @runId",
                                 conn))
                {
                    cmd.Parameters.AddWithValue("runId", dtoRunId);
                    cmd.Parameters.AddWithValue("endOfRun", formattedEndingTime);

                    // Calculate timeOfRun
                    var startTime = await GetStartTimeOfRun(conn, dtoRunId);
                    var endTime = DateTime.Parse(formattedEndingTime);
                    var timeOfRun = endTime - startTime;
                    cmd.Parameters.AddWithValue("timeOfRun", timeOfRun.ToString(@"hh\:mm\:ss"));

                    await cmd.ExecuteNonQueryAsync();
                }

                // Insert last coordinates into Corsa.maps
                await using (var cmd = new NpgsqlCommand(
                                 "INSERT INTO Corsa.maps (mapID, lat, lng, time) VALUES (@mapId, @lat, @lng, @time)",
                                 conn))
                {
                    cmd.Parameters.AddWithValue("mapId", dtoRunId);
                    cmd.Parameters.AddWithValue("lat", dtoEndingLat);
                    cmd.Parameters.AddWithValue("lng", dtoEndingLng);
                    cmd.Parameters.AddWithValue("time", formattedEndingTime);

                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
                Console.WriteLine("Ending of run logged successfully.");
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
    }

    private async Task<DateTime> GetStartTimeOfRun(NpgsqlConnection conn, string runId)
    {
        // Query Corsa.runs table to get the startOfRun time
        var queryString = "SELECT startOfRun FROM Corsa.runs WHERE runID = @runId";
        await using var cmd = new NpgsqlCommand(queryString, conn);
        cmd.Parameters.AddWithValue("runId", runId);
        var startTime = await cmd.ExecuteScalarAsync();
        return Convert.ToDateTime(startTime);
    }

    public async Task<string> SaveRunToDb(string runId, double dtoUserId, string formattedRunDateTime, string formattedRunTime, double dtoRunDistance)
    {
        string insertedRunId = string.Empty;
        try
        {
            var connString = Environment.GetEnvironmentVariable("DBConnectionString");
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            await using var transaction = await conn.BeginTransactionAsync();
            try
            {
                // Insert into Corsa.runs
                await using (var cmd = new NpgsqlCommand("INSERT INTO Corsa.runs (runID, user_id, startOfRun, timeOfRun, distance) VALUES (@runId, @userId, @startOfRun, @timeOfRun, @distance) RETURNING runID", conn))
                {
                    cmd.Parameters.AddWithValue("runId", runId);
                    cmd.Parameters.AddWithValue("userId", dtoUserId);
                    cmd.Parameters.AddWithValue("startOfRun", formattedRunDateTime);
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
            var connString = Environment.GetEnvironmentVariable("DBConnectionString");
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            await using var transaction = await conn.BeginTransactionAsync();
            try
            {
                // Check if the run belongs to the user
                var isAuthorized = await IsRunBelongsToUser(conn, dtoUserId, dtoRunId);
                if (!isAuthorized)
                {
                    Console.WriteLine($"User {dtoUserId} is not authorized to delete run {dtoRunId}.");
                    return null;
                }

                // Delete run from Corsa.runs
                await using (var cmd = new NpgsqlCommand("DELETE FROM Corsa.runs WHERE runID = @runId RETURNING runID", conn))
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
        var queryString = "SELECT COUNT(*) FROM Corsa.runs WHERE runID = @runId AND user_id = @userId";
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
            var connString = Environment.GetEnvironmentVariable("DBConnectionString");
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            // Query all runs for the user from Corsa.runs
            var queryString = "SELECT runID, startOfRun, endOfRun, timeOfRun, distance FROM Corsa.runs WHERE user_id = @userId";
            await using var cmd = new NpgsqlCommand(queryString, conn);
            cmd.Parameters.AddWithValue("userId", dtoUserId);

            // Execute the query and read the results
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var runId = reader["runID"].ToString();
                var startOfRun = reader.GetDateTime(reader.GetOrdinal("startOfRun"));
                var endOfRun = reader.IsDBNull(reader.GetOrdinal("endOfRun")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("endOfRun"));
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

}