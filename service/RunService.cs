using System.Globalization;
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
        string? dtoRunStartTime)
    {
        // Remove the '/' and ':' characters from the formattedDateTime string
        string runId = $"{dtoUserId}_{dtoRunStartTime!.Replace("/", "").Replace(":", "").Replace(" ", "")}";
        // Convert dtoRunStartTime to DateTime
        DateTime dateTime = DateTime.ParseExact(dtoRunStartTime!, "dd/MM/yy HH:mm", CultureInfo.InvariantCulture);
        return await _runRepository.LogRunToDb(dtoUserId,runId, dtoStartingLat, dtoStartingLng, dateTime);
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

    public async Task<string> SaveRunToDb(int dtoUserId, string dtoRunDateTime, string dtoRunTime,
        double dtoRunDistance)
    {
        // Convert dtoRunStartTime to DateTime
        DateTime dateTime = DateTime.ParseExact(dtoRunDateTime!, "dd/MM/yy HH:mm", CultureInfo.InvariantCulture);
        // Remove the '/' and ':' characters from the formattedDateTime string
        string runId = $"{dtoUserId}_{dtoRunDateTime!.Replace("/", "").Replace(":", "").Replace(" ", "")}";
        TimeSpan runTime = TimeSpan.Parse(dtoRunTime);
        return await _runRepository.SaveRunToDb(runId, dtoUserId, dateTime, runTime, dtoRunDistance);
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