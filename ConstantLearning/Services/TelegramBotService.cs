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
                
                // Handle /set-repetition-time command
                if (messageText.StartsWith("/set-repetition-time", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleSetRepetitionTimeCommand(messageChatId, messageText);
                    return;
                }
                
                // Handle /set-new-words-time command
                if (messageText.StartsWith("/set-new-words-time", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleSetNewWordsTimeCommand(messageChatId, messageText);
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

    private async Task HandleSetRepetitionTimeCommand(long messageChatId, string messageText)
    {
        try
        {
            var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                var usageMessage = _languageOptions.SourceLanguageCode.ToLower() == "uk"
                    ? "❌ Невірний формат команди.\n\nВикористання: `/set-repetition-time HH:MM`\nПриклад: `/set-repetition-time 09:30`"
                    : "❌ Invalid command format.\n\nUsage: `/set-repetition-time HH:MM`\nExample: `/set-repetition-time 09:30`";
                
                await botClient.SendMessage(chatId: messageChatId, text: usageMessage, parseMode: ParseMode.Markdown);
                return;
            }

            var timeString = parts[1];
            if (!IsValidTimeFormat(timeString))
            {
                var errorMessage = _languageOptions.SourceLanguageCode.ToLower() == "uk"
                    ? "❌ Невірний формат часу. Використовуйте HH:MM (наприклад, 09:30)"
                    : "❌ Invalid time format. Use HH:MM (e.g., 09:30)";
                
                await botClient.SendMessage(chatId: messageChatId, text: errorMessage, parseMode: ParseMode.Markdown);
                return;
            }

            using var scope = serviceProvider.CreateScope();
            var chatRegistrationService = scope.ServiceProvider.GetRequiredService<IChatRegistrationService>();
            
            await chatRegistrationService.UpdateRepetitionTimeAsync(messageChatId, timeString);

            var successMessage = _languageOptions.SourceLanguageCode.ToLower() == "uk"
                ? $"✅ Час повторення встановлено на *{timeString}*\n\nПовторення буде надсилатися щодня о цій годині."
                : $"✅ Repetition time set to *{timeString}*\n\nRepetitions will be sent daily at this time.";

            await botClient.SendMessage(chatId: messageChatId, text: successMessage, parseMode: ParseMode.Markdown);
            
            logger.LogInformation("Repetition time updated to {Time} for chat {ChatId}", timeString, messageChatId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling set-repetition-time command for chat {ChatId}", messageChatId);

            var errorMessage = _languageOptions.SourceLanguageCode.ToLower() == "uk"
                ? "❌ Помилка при оновленні часу. Спробуйте пізніше."
                : "❌ Error updating time. Please try again later.";

            await botClient.SendMessage(chatId: messageChatId, text: errorMessage, parseMode: ParseMode.Markdown);
        }
    }

    private async Task HandleSetNewWordsTimeCommand(long messageChatId, string messageText)
    {
        try
        {
            var parts = messageText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                var usageMessage = _languageOptions.SourceLanguageCode.ToLower() == "uk"
                    ? "❌ Невірний формат команди.\n\nВикористання: `/set-new-words-time HH:MM`\nПриклад: `/set-new-words-time 20:00`"
                    : "❌ Invalid command format.\n\nUsage: `/set-new-words-time HH:MM`\nExample: `/set-new-words-time 20:00`";
                
                await botClient.SendMessage(chatId: messageChatId, text: usageMessage, parseMode: ParseMode.Markdown);
                return;
            }

            var timeString = parts[1];
            if (!IsValidTimeFormat(timeString))
            {
                var errorMessage = _languageOptions.SourceLanguageCode.ToLower() == "uk"
                    ? "❌ Невірний формат часу. Використовуйте HH:MM (наприклад, 20:00)"
                    : "❌ Invalid time format. Use HH:MM (e.g., 20:00)";
                
                await botClient.SendMessage(chatId: messageChatId, text: errorMessage, parseMode: ParseMode.Markdown);
                return;
            }

            using var scope = serviceProvider.CreateScope();
            var chatRegistrationService = scope.ServiceProvider.GetRequiredService<IChatRegistrationService>();
            
            await chatRegistrationService.UpdateNewWordsTimeAsync(messageChatId, timeString);

            var successMessage = _languageOptions.SourceLanguageCode.ToLower() == "uk"
                ? $"✅ Час нових слів встановлено на *{timeString}*\n\nНові слова будуть надсилатися щодня о цій годині."
                : $"✅ New words time set to *{timeString}*\n\nNew words will be sent daily at this time.";

            await botClient.SendMessage(chatId: messageChatId, text: successMessage, parseMode: ParseMode.Markdown);
            
            logger.LogInformation("New words time updated to {Time} for chat {ChatId}", timeString, messageChatId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling set-new-words-time command for chat {ChatId}", messageChatId);

            var errorMessage = _languageOptions.SourceLanguageCode.ToLower() == "uk"
                ? "❌ Помилка при оновленні часу. Спробуйте пізніше."
                : "❌ Error updating time. Please try again later.";

            await botClient.SendMessage(chatId: messageChatId, text: errorMessage, parseMode: ParseMode.Markdown);
        }
    }

    private static bool IsValidTimeFormat(string timeString)
    {
        if (string.IsNullOrWhiteSpace(timeString))
            return false;

        var parts = timeString.Split(':');
        if (parts.Length != 2)
            return false;

        if (!int.TryParse(parts[0], out var hour) || !int.TryParse(parts[1], out var minute))
            return false;

        return hour >= 0 && hour <= 23 && minute >= 0 && minute <= 59;
    }

    private async Task HandleHelpCommand(long messageChatId)
    {
        var message = _languageOptions.SourceLanguageCode.ToLower() == "uk"
            ? @"📚 *Бот для вивчення мов*

🔹 Доступні команди:
• `/start-learning` - Розпочати навчання в цій групі
• `/stop-learning` - Зупинити навчання
• `/restart-progress` - Скинути прогрес навчання
• `/set-repetition-time HH:MM` - Встановити час повторення (наприклад, `/set-repetition-time 09:30`)
• `/set-new-words-time HH:MM` - Встановити час нових слів (наприклад, `/set-new-words-time 20:00`)

📝 Після реєстрації бот автоматично надсилає:
• Повторення вивчених слів
• Нові слова для вивчення

⏰ Ви можете налаштувати графік відправки за допомогою команд вище."
            : @"📚 *Language Learning Bot*

🔹 Available commands:
• `/start-learning` - Start learning in this chat
• `/stop-learning` - Stop learning
• `/restart-progress` - Reset learning progress
• `/set-repetition-time HH:MM` - Set repetition time (e.g., `/set-repetition-time 09:30`)
• `/set-new-words-time HH:MM` - Set new words time (e.g., `/set-new-words-time 20:00`)

📝 After registration, the bot automatically sends:
• Repetition of learned words
• New words to learn

⏰ You can configure the schedule using the commands above.";

        await botClient.SendMessage(chatId: messageChatId, text: message, parseMode: ParseMode.Markdown);
    }
}