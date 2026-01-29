using ConstantLearning.Data;
using Microsoft.EntityFrameworkCore;

namespace ConstantLearning.Services;

public interface IProgressService
{
    Task<int> RestartProgressAsync(long chatId);
}

public class ProgressService : IProgressService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ProgressService> _logger;

    public ProgressService(AppDbContext context, ILogger<ProgressService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<int> RestartProgressAsync(long chatId)
    {
        _logger.LogInformation("Restarting learning progress for chat {ChatId}", chatId);

        var learnedWords = await _context.LearnedWords
            .Where(lw => lw.ChatId == chatId)
            .ToListAsync();
        
        var count = learnedWords.Count;
        
        _context.LearnedWords.RemoveRange(learnedWords);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Progress restarted for chat {ChatId}. Removed {Count} learned words", chatId, count);

        return count;
    }
}
