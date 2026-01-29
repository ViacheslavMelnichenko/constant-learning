using ConstantLearning.Configuration;
using ConstantLearning.Services;
using Microsoft.Extensions.Options;
using Quartz;

namespace ConstantLearning.Jobs;

[DisallowConcurrentExecution]
public class NewWordsJob(
    IWordService wordService,
    ITelegramBotService telegramBotService,
    IMessageFormatterService messageFormatterService,
    ILogger<NewWordsJob> logger,
    IOptions<LearningOptions> learningOptions,
    IOptions<LanguageOptions> languageOptions,
    IServiceProvider serviceProvider)
    : IJob
{
    private readonly LearningOptions _learningOptions = learningOptions.Value;
    private readonly LanguageOptions _languageOptions = languageOptions.Value;

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            logger.LogInformation("Starting new words job for all registered chats");

            // Get all active chats
            using var scope = serviceProvider.CreateScope();
            var chatRegistrationService = scope.ServiceProvider.GetRequiredService<IChatRegistrationService>();
            var activeChatIds = await chatRegistrationService.GetAllActiveChatIdsAsync();

            if (activeChatIds.Count == 0)
            {
                logger.LogInformation("No registered chats found. Skipping new words job.");
                return;
            }

            logger.LogInformation("Processing new words for {Count} registered chat(s)", activeChatIds.Count);

            foreach (var chatId in activeChatIds)
            {
                try
                {
                    await ProcessNewWordsForChatAsync(chatId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing new words for chat {ChatId}", chatId);
                    // Continue with other chats
                }
            }

            logger.LogInformation("New words job completed for all chats");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing new words job");
            throw;
        }
    }

    private async Task ProcessNewWordsForChatAsync(long chatId)
    {
        logger.LogInformation("Starting new words for chat {ChatId}", chatId);

        var words = await wordService.GetNewWordsAsync(chatId, _learningOptions.NewWordsCount);

        if (words.Count == 0)
        {
            logger.LogInformation("No new words available for chat {ChatId}", chatId);
            var completionMessage = _languageOptions.SourceLanguageCode.Equals("uk", StringComparison.CurrentCultureIgnoreCase)
                ? "Всі слова вже вивчені! 🎉"
                : "All words already learned! 🎉";
            await telegramBotService.SendMessageAsync(completionMessage, chatId);
            return;
        }

        // Send new words
        var message = messageFormatterService.FormatNewWords(words);
        await telegramBotService.SendMessageAsync(message, chatId);

        // Mark as learned
        await wordService.MarkWordsAsLearnedAsync(chatId, words.Select(w => w.Id));

        logger.LogInformation("New words completed for chat {ChatId}. Words learned: {Count}", chatId, words.Count);
    }
}