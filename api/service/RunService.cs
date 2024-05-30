using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Backend.exceptions;
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
        var loggedId = await _runRepository.LogRunToDb(dtoUserId,runId, dtoStartingLat, dtoStartingLng, dtoRunStartTime);
        if (loggedId is null)
            throw new LoggingOfRunFailedException("Logging of run failed");
        return loggedId;
    }

    public async Task LogCoordinatesToDb(string dtoRunId, double dtoLat, double dtoLng, DateTime dtoLoggingTime)
    {
        await _runRepository.LogCoordinatesToDb(dtoRunId, dtoLat, dtoLng, dtoLoggingTime);
    }
    
    public async Task<RunInfoWithMap> LogEndingOfRunToDb(string dtoRunId, double dtoEndingLat, double dtoEndingLng,
        DateTime dtoRunEndTime)
    {
        DistanceCalculator dc = new DistanceCalculator();
        var runInfo = await _runRepository.LogEndingOfRunToDb(dtoRunId, dtoEndingLat, dtoEndingLng, dtoRunEndTime);

        if (runInfo.Distance is 0 or null)
        {
            double totalDistance = 0;
            for (int i = 0; i < runInfo.gpsCordsList.Count - 1; i++)
            {
                var coord1 = runInfo.gpsCordsList[i];
                var coord2 = runInfo.gpsCordsList[i + 1];
                totalDistance += dc.CalculateDistance(coord1.Latitude, coord1.Longitude, coord2.Latitude, coord2.Longitude);
            }

            // Set the distance property
            runInfo.Distance = totalDistance;
            
            // Update the distance in the database
            await _runRepository.UpdateDistanceOfRun(dtoRunId, totalDistance);

            return runInfo;
        }

        return runInfo;
       
        
    }

    public async Task<string> SaveRunToDb(int dtoUserId, string dtoRunDateTime, TimeSpan dtoRunTime,
        double dtoRunDistance)
    {
        // Convert dtoRunStartTime to DateTime
        DateTime dateTime = DateTime.ParseExact(dtoRunDateTime!, "dd/MM/yy HH:mm", CultureInfo.InvariantCulture);
        // Remove the '/' and ':' characters from the formattedDateTime string
        string runId = $"{dtoUserId}_{dtoRunDateTime!.Replace("/", "").Replace(":", "").Replace(" ", "")}";
        return await _runRepository.SaveRunToDb(runId, dtoUserId, dateTime, dtoRunTime, dtoRunDistance);
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

public class DistanceCalculator
{
    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371e3; // metres
        var φ1 = lat1 * Math.PI/180; // φ, λ in radians
        var φ2 = lat2 * Math.PI/180;
        var Δφ = (lat2-lat1) * Math.PI/180;
        var Δλ = (lon2-lon1) * Math.PI/180;

        var a = Math.Sin(Δφ/2) * Math.Sin(Δφ/2) +
                Math.Cos(φ1) * Math.Cos(φ2) *
                Math.Sin(Δλ/2) * Math.Sin(Δλ/2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a));

        var distance = R * c; // in metres
        return distance;
    }
}