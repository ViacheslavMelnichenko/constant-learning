namespace ConstantLearning.Configuration;

public class WordsImportOptions
{
    public const string SectionName = "WordsImport";
    
    public required string WordsFolder { get; set; }
}
