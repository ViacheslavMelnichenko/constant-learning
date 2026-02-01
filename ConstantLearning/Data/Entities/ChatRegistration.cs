namespace ConstantLearning.Data.Entities;

public class ChatRegistration
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public string? ChatTitle { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string RepetitionTime { get; set; } = "09:00";
    public string NewWordsTime { get; set; } = "20:00";
    public int NewWordsCount { get; set; } = 3;
    public int RepetitionWordsCount { get; set; } = 10;
}
