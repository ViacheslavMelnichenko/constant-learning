namespace ConstantLearning.Configuration;

public class ScheduleOptions
{
    public const string SectionName = "Schedule";
    
    public required string RepetitionCron { get; set; }
    public required string NewWordsCron { get; set; }
}
