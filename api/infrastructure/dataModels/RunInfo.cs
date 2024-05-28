namespace Backend.infrastructure.dataModels;

public class RunInfo
{
    public string RunId { get; set; }
    public DateTime StartOfRun { get; set; }
    public DateTime? EndOfRun { get; set; }
    public string TimeOfRun { get; set; }
    public double Distance { get; set; }
}

public class RunInfoWithMap : RunInfo
{
    public List<Coordinates> Coordinates { get; set; }
}