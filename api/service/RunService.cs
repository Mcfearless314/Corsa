using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
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
        DateTime dtoRunStartTime)
    {
        string runStartTime = dtoRunStartTime.ToString("s");
        
        // Remove the '/' and ':' characters from the formattedDateTime string
        string runId = $"{dtoUserId}_{runStartTime!.Replace("/", "").Replace(":", "").Replace(" ", "")}";
        return await _runRepository.LogRunToDb(dtoUserId,runId, dtoStartingLat, dtoStartingLng, dtoRunStartTime);
    }

    public async Task LogCoordinatesToDb(string dtoRunId, double dtoLat, double dtoLng, DateTime dtoLoggingTime)
    {
        await _runRepository.LogCoordinatesToDb(dtoRunId, dtoLat, dtoLng, dtoLoggingTime);
    }
    
    public async Task<RunInfoWithMap> LogEndingOfRunToDb(string dtoRunId, double dtoEndingLat, double dtoEndingLng,
        DateTime dtoRunEndTime)
    {
        string runStartTime = dtoRunEndTime.ToString("s");
        return await _runRepository.LogEndingOfRunToDb(dtoRunId, dtoEndingLat, dtoEndingLng, runStartTime);
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

    public async Task<RunInfoWithMap> GetFullInfoOfRun(int dtoUserId, string dtoRunId)
    {   
        return await _runRepository.GetFullInfoOfRun(dtoUserId, dtoRunId);
    }
}