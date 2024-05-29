using Backend.exceptions;
using Backend.infrastructure.dataModels;
using Npgsql;

namespace Backend.infrastructure.Repositories;

public class DeviceRepository
{
    private NpgsqlDataSource _dataSource;

    public DeviceRepository(NpgsqlDataSource datasource)
    {
        _dataSource = datasource;
    }

    public async Task<int> GetUserIdByDevice(string dtoDeviceId)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand("SELECT user_id FROM corsa.devices WHERE deviceID = @deviceId",
                connection);
            cmd.Parameters.AddWithValue("deviceId", dtoDeviceId);

            var userId = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(userId);
        }
        catch (Exception e)
        {
            throw new DeviceNotRegisteredException("Device has no user attached");
        }
    }

    public async Task LogCoordinates(string runId, int userId, DateTime runStartTime, DateTime runEndTime,
        TimeSpan timeOfRun, List<Coordinates> dtoCoordinates)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                await using (var cmd = new NpgsqlCommand(
                                 "INSERT INTO corsa.runs (runID, user_id, startOfRun, endOfRun, timeOfRun) VALUES (@runId, @userId, @runStartTime, @runEndTime, @timeOfRun)",
                                 connection))
                {
                    cmd.Parameters.AddWithValue("runId", runId);
                    cmd.Parameters.AddWithValue("userId", userId);
                    cmd.Parameters.AddWithValue("runStartTime", runStartTime);
                    cmd.Parameters.AddWithValue("runEndTime", runEndTime);
                    cmd.Parameters.AddWithValue("timeOfRun", timeOfRun);
                    await cmd.ExecuteNonQueryAsync();
                }

                foreach (var coordinate in dtoCoordinates)
                {
                    await using (var cmd = new NpgsqlCommand(
                                     "INSERT INTO corsa.maps (mapID, lat, lng, time) VALUES (@mapId, @lat, @lng, @time)",
                                     connection))
                    {
                        cmd.Parameters.AddWithValue("mapId", runId);
                        cmd.Parameters.AddWithValue("lat", coordinate.Latitude);
                        cmd.Parameters.AddWithValue("lng", coordinate.Longitude);
                        cmd.Parameters.AddWithValue("time", coordinate.TimeStamp);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        catch (Exception e)
        {
            throw new Exception("Error occurred while logging coordinates: " + e.Message);
        }
    }

    public async Task<string> IsDeviceRegisteredInDb(string deviceId)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            await using var cmd = new NpgsqlCommand("SELECT deviceId FROM corsa.devices WHERE deviceID = @deviceId",
                connection);
            cmd.Parameters.AddWithValue("deviceId", deviceId);

            string deviceIdInDb = (string)await cmd.ExecuteScalarAsync();
            return deviceIdInDb;
        }
        catch (Exception e)
        {
            throw new DeviceNotRegisteredException("Device has no user attached");
        }
    }
}