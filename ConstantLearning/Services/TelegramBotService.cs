using ConstantLearning.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ConstantLearning.Services;

public interface ITelegramBotService
{
    Task SendMessageAsync(string text, long chatId, ParseMode parseMode = ParseMode.Markdown);
    Task HandleUpdateAsync(Update update);
}

public class TelegramBotService(
    ITelegramBotClient botClient,
    IOptions<LanguageOptions> languageOptions,
    ILogger<TelegramBotService> logger,
    IServiceProvider serviceProvider)
    : ITelegramBotService
{
    private readonly LanguageOptions _languageOptions = languageOptions.Value;
    public async Task SendMessageAsync(string text, long chatId, ParseMode parseMode = ParseMode.Markdown)
    {
        try
        {
            await botClient.SendMessage(
                chatId: chatId,
                text: text,
                parseMode: parseMode);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send message to chat {ChatId}", chatId);
            throw;
        }
    }

    public async Task HandleUpdateAsync(Update update)
    {
        try
        {
            if (update.Type == UpdateType.Message && update.Message?.Text != null)
            {
                var messageText = update.Message.Text.Trim();
                var messageChatId = update.Message.Chat.Id;
                
                logger.LogInformation("Received message from chat {ChatId}: {MessageText}", messageChatId, messageText);

                // Handle /restart-progress command
                if (messageText.Equals("/restart-progress", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleRestartProgressCommand(messageChatId);
                    return;
                }
                
                // Handle /help command
                if (messageText.Equals("/help", StringComparison.OrdinalIgnoreCase) || 
                    messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleHelpCommand(messageChatId);
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling update");
        }

        await Task.CompletedTask;
    }

    private async Task HandleRestartProgressCommand(long messageChatId)
    {
        try
        {
            logger.LogInformation("Processing /restart-progress command for chat {ChatId}", messageChatId);

            // Create a scope to get scoped services
            using var scope = serviceProvider.CreateScope();
            var progressService = scope.ServiceProvider.GetRequiredService<IProgressService>();

            var removedCount = await progressService.RestartProgressAsync(messageChatId);

            var message = _languageOptions.SourceLanguageCode.ToLower() == "uk"
                ? $"✅ Прогрес скинуто!\n\nВидалено {removedCount} вивчених слів.\nПочинаємо навчання спочатку! 🎯"
                : $"✅ Progress restarted!\n\nRemoved {removedCount} learned words.\nStarting from scratch! 🎯";

            await botClient.SendMessage(chatId: messageChatId, text: message, parseMode: ParseMode.Markdown);

            logger.LogInformation("Progress restart completed for chat {ChatId}. Removed {Count} words", messageChatId, removedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling restart-progress command for chat {ChatId}", messageChatId);

            var errorMessage = _languageOptions.SourceLanguageCode.ToLower() == "uk"
                ? "❌ Помилка при скиданні прогресу. Спробуйте пізніше."
                : "❌ Error restarting progress. Please try again later.";

            await botClient.SendMessage(chatId: messageChatId, text: errorMessage, parseMode: ParseMode.Markdown);
        }
    }

    private async Task HandleHelpCommand(long messageChatId)
    {
        var message = _languageOptions.SourceLanguageCode.ToLower() == "uk"
            ? @"📚 *Бот для вивчення мов*

🔹 Доступні команди:
• `/start-learning` - Розпочати навчання в цій групі
• `/stop-learning` - Зупинити навчання
• `/restart-progress` - Скинути прогрес навчання

📝 Після реєстрації бот автоматично надсилає:
• Повторення вивчених слів
• Нові слова для вивчення

Графік відправки налаштовано в конфігурації."
            : @"📚 *Language Learning Bot*

🔹 Available commands:
• `/start-learning` - Start learning in this chat
• `/stop-learning` - Stop learning
• `/restart-progress` - Reset learning progress

📝 After registration, the bot automatically sends:
• Repetition of learned words
• New words to learn

Schedule is configured in settings.";

        await botClient.SendMessage(chatId: messageChatId, text: message, parseMode: ParseMode.Markdown);
    }
}