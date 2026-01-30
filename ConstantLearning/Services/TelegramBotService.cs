using ConstantLearning.Enums;
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
    IBotMessagesService botMessages,
    ILogger<TelegramBotService> logger,
    IServiceProvider serviceProvider)
    : ITelegramBotService
{
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

                if (messageText.Equals("/start-learning", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleStartLearningCommand(messageChatId, update.Message.Chat.Title);
                    return;
                }

                if (messageText.Equals("/stop-learning", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleStopLearningCommand(messageChatId);
                    return;
                }

                if (messageText.Equals("/restart-progress", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleRestartProgressCommand(messageChatId);
                    return;
                }

                if (messageText.StartsWith("/set-repetition-time", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleSetRepetitionTimeCommand(messageChatId, messageText);
                    return;
                }

                if (messageText.StartsWith("/set-new-words-time", StringComparison.OrdinalIgnoreCase))
                {
                    await HandleSetNewWordsTimeCommand(messageChatId, messageText);
                    return;
                }

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

    private async Task HandleStartLearningCommand(long messageChatId, string? chatTitle)
    {
        try
        {
            logger.LogInformation("Processing /start-learning command for chat {ChatId}", messageChatId);

            using var scope = serviceProvider.CreateScope();
            var chatRegistrationService = scope.ServiceProvider.GetRequiredService<IChatRegistrationService>();

            var isAlreadyRegistered = await chatRegistrationService.IsChatRegisteredAsync(messageChatId);

            if (isAlreadyRegistered)
            {
                var message = botMessages.GetMessage(BotMessageKey.ChatAlreadyRegistered);
                await botClient.SendMessage(chatId: messageChatId, text: message, parseMode: ParseMode.Markdown);
                return;
            }

            await chatRegistrationService.RegisterChatAsync(messageChatId, chatTitle);

            var successMessage = botMessages.GetMessage(BotMessageKey.ChatRegisteredSuccess);
            await botClient.SendMessage(chatId: messageChatId, text: successMessage, parseMode: ParseMode.Markdown);

            logger.LogInformation("Chat {ChatId} ({ChatTitle}) successfully registered", messageChatId, chatTitle);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling start-learning command for chat {ChatId}", messageChatId);

            var errorMessage = botMessages.GetMessage(BotMessageKey.RegistrationError);
            await botClient.SendMessage(chatId: messageChatId, text: errorMessage, parseMode: ParseMode.Markdown);
        }
    }

    private async Task HandleStopLearningCommand(long messageChatId)
    {
        try
        {
            logger.LogInformation("Processing /stop-learning command for chat {ChatId}", messageChatId);

            using var scope = serviceProvider.CreateScope();
            var chatRegistrationService = scope.ServiceProvider.GetRequiredService<IChatRegistrationService>();

            if (!await EnsureChatRegisteredAsync(messageChatId, chatRegistrationService))
                return;

            await chatRegistrationService.DeactivateChatAsync(messageChatId);

            var successMessage = botMessages.GetMessage(BotMessageKey.LearningStopped);
            await botClient.SendMessage(chatId: messageChatId, text: successMessage, parseMode: ParseMode.Markdown);

            logger.LogInformation("Chat {ChatId} deactivated", messageChatId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling stop-learning command for chat {ChatId}", messageChatId);

            var errorMessage = botMessages.GetMessage(BotMessageKey.StopLearningError);
            await botClient.SendMessage(chatId: messageChatId, text: errorMessage, parseMode: ParseMode.Markdown);
        }
    }

    private async Task HandleRestartProgressCommand(long messageChatId)
    {
        try
        {
            logger.LogInformation("Processing /restart-progress command for chat {ChatId}", messageChatId);

            using var scope = serviceProvider.CreateScope();
            var progressService = scope.ServiceProvider.GetRequiredService<IProgressService>();

            var removedCount = await progressService.RestartProgressAsync(messageChatId);

            var message = botMessages.GetMessage(BotMessageKey.ProgressRestarted, removedCount);
            await botClient.SendMessage(chatId: messageChatId, text: message, parseMode: ParseMode.Markdown);

            logger.LogInformation("Progress restart completed for chat {ChatId}. Removed {Count} words", messageChatId,
                removedCount);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling restart-progress command for chat {ChatId}", messageChatId);

            var errorMessage = botMessages.GetMessage(BotMessageKey.RestartProgressError);
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
                var usageMessage = botMessages.GetMessage(BotMessageKey.InvalidTimeCommandFormat,
                    "/set-repetition-time HH:MM", "/set-repetition-time 09:30");
                await botClient.SendMessage(chatId: messageChatId, text: usageMessage, parseMode: ParseMode.Markdown);
                return;
            }

            var timeString = parts[1];
            if (!IsValidTimeFormat(timeString))
            {
                var errorMessage = botMessages.GetMessage(BotMessageKey.InvalidTimeFormat, "09:30");
                await botClient.SendMessage(chatId: messageChatId, text: errorMessage, parseMode: ParseMode.Markdown);
                return;
            }

            using var scope = serviceProvider.CreateScope();
            var chatRegistrationService = scope.ServiceProvider.GetRequiredService<IChatRegistrationService>();

            if (!await EnsureChatRegisteredAsync(messageChatId, chatRegistrationService))
                return;

            await chatRegistrationService.UpdateRepetitionTimeAsync(messageChatId, timeString);

            var successMessage = botMessages.GetMessage(BotMessageKey.RepetitionTimeSet, timeString);
            await botClient.SendMessage(chatId: messageChatId, text: successMessage, parseMode: ParseMode.Markdown);

            logger.LogInformation("Repetition time updated to {Time} for chat {ChatId}", timeString, messageChatId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling set-repetition-time command for chat {ChatId}. Message: {Message}",
                messageChatId, ex.Message);

            var errorMessage = botMessages.GetMessage(BotMessageKey.UpdateTimeError);
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
                var usageMessage = botMessages.GetMessage(BotMessageKey.InvalidTimeCommandFormat,
                    "/set-new-words-time HH:MM", "/set-new-words-time 20:00");
                await botClient.SendMessage(chatId: messageChatId, text: usageMessage, parseMode: ParseMode.Markdown);
                return;
            }

            var timeString = parts[1];
            if (!IsValidTimeFormat(timeString))
            {
                var errorMessage = botMessages.GetMessage(BotMessageKey.InvalidTimeFormat, "20:00");
                await botClient.SendMessage(chatId: messageChatId, text: errorMessage, parseMode: ParseMode.Markdown);
                return;
            }

            using var scope = serviceProvider.CreateScope();
            var chatRegistrationService = scope.ServiceProvider.GetRequiredService<IChatRegistrationService>();

            if (!await EnsureChatRegisteredAsync(messageChatId, chatRegistrationService))
                return;

            await chatRegistrationService.UpdateNewWordsTimeAsync(messageChatId, timeString);

            var successMessage = botMessages.GetMessage(BotMessageKey.NewWordsTimeSet, timeString);
            await botClient.SendMessage(chatId: messageChatId, text: successMessage, parseMode: ParseMode.Markdown);

            logger.LogInformation("New words time updated to {Time} for chat {ChatId}", timeString, messageChatId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling set-new-words-time command for chat {ChatId}. Message: {Message}",
                messageChatId, ex.Message);

            var errorMessage = botMessages.GetMessage(BotMessageKey.UpdateTimeError);
            await botClient.SendMessage(chatId: messageChatId, text: errorMessage, parseMode: ParseMode.Markdown);
        }
    }

    /// <summary>
    /// Checks if chat is registered and sends error message if not
    /// </summary>
    /// <returns>True if chat is registered, false otherwise</returns>
    private async Task<bool> EnsureChatRegisteredAsync(long chatId, IChatRegistrationService chatRegistrationService)
    {
        var isRegistered = await chatRegistrationService.IsChatRegisteredAsync(chatId);

        if (!isRegistered)
        {
            var message = botMessages.GetMessage(BotMessageKey.ChatNotRegistered);
            await botClient.SendMessage(chatId: chatId, text: message, parseMode: ParseMode.Markdown);
        }

        return isRegistered;
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
        var message = botMessages.GetMessage(BotMessageKey.Help);
        await botClient.SendMessage(chatId: messageChatId, text: message, parseMode: ParseMode.Markdown);
    }
}