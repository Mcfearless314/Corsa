using Backend.infrastructure.dataModels;
using Backend.infrastructure.Repositories;

namespace Backend.service;

public class RunService
{
    private readonly RunRepository _runRepository;

    public RunService(RunRepository runRepository)
    {
        _runRepository = runRepository;
    }

    public async Task<string> LogRunToDb(int dtoUserId, double dtoStartingLat, double dtoStartingLng,
        DateTime? dtoRunStartTime)
    {
        string formattedDateTime = dtoRunStartTime.ToString();
        string runId = $"{dtoUserId}_{formattedDateTime}";
        return await _runRepository.LogRunToDb(runId, dtoStartingLat, dtoStartingLng, formattedDateTime);
    }

    public async Task LogCoordinatesToDb(string dtoRunId, double dtoLat, double dtoLng, DateTime dtoLoggingTime)
    {
        string formattedLoggingTime = dtoLoggingTime.ToString();
        await _runRepository.LogCoordinatesToDb(dtoRunId, dtoLat, dtoLng, formattedLoggingTime);
    }

    public async Task LogEndingOfRunToDb(string dtoRunId, double dtoEndingLat, double dtoEndingLng,
        DateTime dtoRunEndTime)
    {
        string formattedEndingTime = dtoRunEndTime.ToString();
        await _runRepository.LogEndingOfRunToDb(dtoRunId, dtoEndingLat, dtoEndingLng, formattedEndingTime);
    }

    public async Task<string> SaveRunToDb(int dtoUserId, DateTime dtoRunDateTime, string dtoRunTime,
        double dtoRunDistance)
    {
        string formattedRunDateTime = dtoRunDateTime.ToString();
        string runId = $"{dtoUserId}_{formattedRunDateTime}";
        return await _runRepository.SaveRunToDb(runId, dtoUserId, formattedRunDateTime, dtoRunTime, dtoRunDistance);
    }

    public async Task<string> DeleteRunFromDb(int dtoUserId, string dtoRunId)
    {
        return await _runRepository.DeleteRunFromDb(dtoUserId, dtoRunId);
    }

    public async Task<List<RunInfo>> GetAllRunsForUser(int dtoUserId)
    {
        return await _runRepository.GetAllRunsForUser(dtoUserId);
    }

    public async Task<List<ProgressInfo>> GetProgressOfRunsForUser(int dtoUserId)
    {
        return await _runRepository.GetProgressOfRunsForUser(dtoUserId);
    }
}