using Backend.exceptions;
using Backend.infrastructure.dataModels;
using Backend.infrastructure.Repositories;

namespace Backend.service;

public class DeviceService
{
    private readonly DeviceRepository _deviceRepository;

    public DeviceService(DeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public async Task LogCoordinates(string dtoDeviceId, List<Cords> dtoCoordinates)
    {
       var userId = await _deviceRepository.GetUserIdByDevice(dtoDeviceId);
       
       var runStartTime = dtoCoordinates[0].TimeStamp;
       
       var runEndTime = dtoCoordinates[^1].TimeStamp;

       var formattedRunStartTime = runStartTime.ToString("s");
       
       var timeOfRun = runEndTime - runStartTime;

       string runId = $"{userId}_{formattedRunStartTime.Replace("/", "").Replace(":", "").Replace(" ", "")}";
       
       await _deviceRepository.LogCoordinates(runId, userId, runStartTime, runEndTime, timeOfRun, dtoCoordinates);
    }

    public async Task<bool> IsDeviceRegisteredInDb(string dtoDeviceId)
    {
        try
        {
            await _deviceRepository.GetUserIdByDevice(dtoDeviceId);
            return true;
        }
        catch (DeviceNotRegisteredException)
        {
            return false;
        }
    }
}