using ConstantLearning.Data;
using ConstantLearning.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstantLearning.Services;

public interface IChatRegistrationService
{
    Task<bool> IsChatRegisteredAsync(long chatId);
    Task<ChatRegistration> RegisterChatAsync(long chatId, string? chatTitle);
    Task<List<long>> GetAllActiveChatIdsAsync();
    Task DeactivateChatAsync(long chatId);
    Task UpdateRepetitionTimeAsync(long chatId, string time);
    Task UpdateNewWordsTimeAsync(long chatId, string time);
    Task<ChatRegistration?> GetChatRegistrationAsync(long chatId);
}

public class ChatRegistrationService : IChatRegistrationService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ChatRegistrationService> _logger;

    public ChatRegistrationService(AppDbContext context, ILogger<ChatRegistrationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<bool> IsChatRegisteredAsync(long chatId)
    {
        return await _context.ChatRegistrations
            .AnyAsync(cr => cr.ChatId == chatId && cr.IsActive);
    }

    public async Task<ChatRegistration> RegisterChatAsync(long chatId, string? chatTitle)
    {
        var existing = await _context.ChatRegistrations
            .FirstOrDefaultAsync(cr => cr.ChatId == chatId);

        if (existing != null)
        {
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.ChatTitle = chatTitle;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Reactivated chat {ChatId} ({ChatTitle})", chatId, chatTitle);
            }
            return existing;
        }

        var registration = new ChatRegistration
        {
            ChatId = chatId,
            ChatTitle = chatTitle,
            RegisteredAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ChatRegistrations.Add(registration);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Registered new chat {ChatId} ({ChatTitle})", chatId, chatTitle);
        return registration;
    }

    public async Task<List<long>> GetAllActiveChatIdsAsync()
    {
        return await _context.ChatRegistrations
            .Where(cr => cr.IsActive)
            .Select(cr => cr.ChatId)
            .ToListAsync();
    }

    public async Task DeactivateChatAsync(long chatId)
    {
        var registration = await _context.ChatRegistrations
            .FirstOrDefaultAsync(cr => cr.ChatId == chatId);

        if (registration != null)
        {
            registration.IsActive = false;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Deactivated chat {ChatId}", chatId);
        }
    }

    public async Task UpdateRepetitionTimeAsync(long chatId, string time)
    {
        var registration = await _context.ChatRegistrations
            .FirstOrDefaultAsync(cr => cr.ChatId == chatId && cr.IsActive);

        if (registration == null)
        {
            throw new InvalidOperationException($"Chat {chatId} is not registered or not active");
        }

        registration.RepetitionTime = time;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated repetition time to {Time} for chat {ChatId}", time, chatId);
    }

    public async Task UpdateNewWordsTimeAsync(long chatId, string time)
    {
        var registration = await _context.ChatRegistrations
            .FirstOrDefaultAsync(cr => cr.ChatId == chatId && cr.IsActive);

        if (registration == null)
        {
            throw new InvalidOperationException($"Chat {chatId} is not registered or not active");
        }

        registration.NewWordsTime = time;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated new words time to {Time} for chat {ChatId}", time, chatId);
    }

    public async Task<ChatRegistration?> GetChatRegistrationAsync(long chatId)
    {
        return await _context.ChatRegistrations
            .FirstOrDefaultAsync(cr => cr.ChatId == chatId && cr.IsActive);
    }
}
