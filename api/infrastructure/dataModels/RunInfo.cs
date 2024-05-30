namespace Backend.infrastructure.dataModels;

public class RunInfo
{
    public string RunId { get; set; }
    public DateTime StartOfRun { get; set; }
    public DateTime? EndOfRun { get; set; }
    public TimeSpan TimeOfRun { get; set; }
    public double Distance { get; set; }
}

public class RunInfoWithMap
{
    public string RunId { get; set; }
    public DateTime StartOfRun { get; set; }
    public DateTime? EndOfRun { get; set; }
    public TimeSpan TimeOfRun { get; set; }
    public double? Distance { get; set; }
    public List<Cords> gpsCordsList { get; set; }
}