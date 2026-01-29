namespace ConstantLearning.Data.Entities;

public class ChatRegistration
{
    public int Id { get; set; }
    public long ChatId { get; set; }
    public string? ChatTitle { get; set; }
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
