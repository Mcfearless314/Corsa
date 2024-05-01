using Backend.infrastructure.Repositories;

namespace Backend.service;

public class RunService
{
    private readonly RunRepository _runRepository;

    public RunService(RunRepository runRepository)
    {
        _runRepository = runRepository;
    }

    public async Task<string> LogRunToDb(double dtoUserId, double dtoStartingLat, double dtoStartingLng,
        DateTime? dtoRunStartTime)
    {
        string formattedDateTime = dtoRunStartTime.ToString();
        string runId = $"{dtoUserId}_{formattedDateTime}";
        return await _runRepository.LogRunToDb(runId, dtoStartingLat, dtoStartingLng, formattedDateTime);
    }

    public async Task LogCoordinatesToDb(double dtoRunId, double dtoLat, double dtoLng, DateTime dtoLoggingTime)
    {
        string formattedLoggingTime = dtoLoggingTime.ToString();
        await _runRepository.LogCoordinatesToDb(dtoRunId, dtoLat, dtoLng, formattedLoggingTime);
    }

    public async Task LogEndingOfRunToDb(double dtoRunId, double dtoEndingLat, double dtoEndingLng,
        DateTime dtoRunEndTime)
    {
       string formattedEndingTime = dtoRunEndTime.ToString();
       await _runRepository.LogEndingOfRunToDb(dtoRunId, dtoEndingLat, dtoEndingLng, formattedEndingTime);
    }
}