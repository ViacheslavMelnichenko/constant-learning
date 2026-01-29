namespace ConstantLearning.Configuration;

public class LanguageOptions
{
    public const string SectionName = "Language";
    
    public required string TargetLanguage { get; set; }
    public required string SourceLanguage { get; set; }
    public required string TargetLanguageCode { get; set; }
    public required string SourceLanguageCode { get; set; }
}
