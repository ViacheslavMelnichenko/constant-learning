namespace ConstantLearning.Data.Entities;

public class LearnedWord
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public int WordId { get; set; }
    public Word Word { get; set; } = null!;
    public DateTime LearnedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastRepeatedAt { get; set; } = DateTime.UtcNow;
    public int RepetitionCount { get; set; } = 0;
}
