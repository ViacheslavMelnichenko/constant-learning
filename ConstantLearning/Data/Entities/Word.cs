﻿namespace ConstantLearning.Data.Entities;

public class Word
{
    public int Id { get; set; }
    public required string TargetWord { get; set; }
    public required string SourceMeaning { get; set; }
    public required string PhoneticTranscription { get; set; }
    public int FrequencyRank { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
}
