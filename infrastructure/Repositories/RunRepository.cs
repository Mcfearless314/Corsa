using Npgsql;

namespace Backend.infrastructure.Repositories;

public class RunRepository
{
    private NpgsqlDataSource _dataSource;

    public RunRepository(NpgsqlDataSource datasource)
    {
        _dataSource = datasource;
    }
    
    public async Task<string> LogRunToDb(string runId, double dtoStartingLat, double dtoStartingLng, string? formattedDateTime)
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
                await using (var cmd = new NpgsqlCommand("INSERT INTO Corsa.runs (runID, startOfRun) VALUES (@runId, @startOfRun) RETURNING runID", conn))
                {
                    cmd.Parameters.AddWithValue("runId", runId);
                    cmd.Parameters.AddWithValue("startOfRun", formattedDateTime);
                    insertedRunId = (string)await cmd.ExecuteScalarAsync();
                }

                // Insert into Corsa.maps
                await using (var cmd = new NpgsqlCommand("INSERT INTO Corsa.maps (mapID, lat, lng, time) VALUES (@mapId, @lat, @lng, @time)", conn))
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


    public async Task LogCoordinatesToDb(double dtoRunId, double dtoLat, double dtoLng, string formattedLoggingTime)
    {
        try
        {
            var connString = Environment.GetEnvironmentVariable("DBConnectionString");
            await using var conn = new NpgsqlConnection(connString);
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand("INSERT INTO Corsa.maps (mapID, lat, lng, time) VALUES (@mapId, @lat, @lng, @time)", conn);
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

   public async Task LogEndingOfRunToDb(double dtoRunId, double dtoEndingLat, double dtoEndingLng, string formattedEndingTime)
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
            await using (var cmd = new NpgsqlCommand("UPDATE Corsa.runs SET endOfRun = @endOfRun, timeOfRun = @timeOfRun WHERE runID = @runId", conn))
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
            await using (var cmd = new NpgsqlCommand("INSERT INTO Corsa.maps (mapID, lat, lng, time) VALUES (@mapId, @lat, @lng, @time)", conn))
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

private async Task<DateTime> GetStartTimeOfRun(NpgsqlConnection conn, double runId)
{
    // Query Corsa.runs table to get the startOfRun time
    var queryString = "SELECT startOfRun FROM Corsa.runs WHERE runID = @runId";
    await using var cmd = new NpgsqlCommand(queryString, conn);
    cmd.Parameters.AddWithValue("runId", runId);
    var startTime = await cmd.ExecuteScalarAsync();
    return Convert.ToDateTime(startTime);
}

}