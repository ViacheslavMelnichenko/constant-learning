using ConstantLearning.Configuration;
using ConstantLearning.Services;
using Microsoft.Extensions.Options;
using Quartz;
using Telegram.Bot.Types.Enums;

namespace ConstantLearning.Jobs;

[DisallowConcurrentExecution]
public class NewWordsJob(
    IWordService wordService,
    ITelegramBotService telegramBotService,
    IMessageFormatterService messageFormatterService,
    IBotMessagesService botMessages,
    ILogger<NewWordsJob> logger,
    IOptions<LearningOptions> learningOptions,
    IServiceProvider serviceProvider)
    : IJob
{
    private readonly LearningOptions _learningOptions = learningOptions.Value;

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            logger.LogInformation("Starting new words job check for all registered chats");

            // Get all active chats
            using var scope = serviceProvider.CreateScope();
            var chatRegistrationService = scope.ServiceProvider.GetRequiredService<IChatRegistrationService>();
            var activeChatIds = await chatRegistrationService.GetAllActiveChatIdsAsync();

            if (activeChatIds.Count == 0)
            {
                logger.LogInformation("No registered chats found. Skipping new words job.");
                return;
            }

            // Get current time in HH:mm format
            var currentTime = DateTime.Now.ToString("HH:mm");
            var currentHour = DateTime.Now.Hour;
            var currentMinute = DateTime.Now.Minute;

            logger.LogInformation("Current time: {CurrentTime}. Checking {Count} chat(s) for scheduled new words", 
                currentTime, activeChatIds.Count);

            var processedCount = 0;

            foreach (var chatId in activeChatIds)
            {
                try
                {
                    var chatRegistration = await chatRegistrationService.GetChatRegistrationAsync(chatId);
                    if (chatRegistration == null)
                        continue;

                    // Parse configured time
                    var configuredTime = chatRegistration.NewWordsTime;
                    var timeParts = configuredTime.Split(':');
                    if (timeParts.Length != 2)
                        continue;

                    var configuredHour = int.Parse(timeParts[0]);
                    var configuredMinute = int.Parse(timeParts[1]);

                    // Check if current time matches configured time (within same hour and minute)
                    if (currentHour == configuredHour && currentMinute == configuredMinute)
                    {
                        await ProcessNewWordsForChatAsync(chatId);
                        processedCount++;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing new words check for chat {ChatId}", chatId);
                    // Continue with other chats
                }
            }

            logger.LogInformation("New words job check completed. Processed {ProcessedCount} chat(s)", processedCount);
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
            var completionMessage = botMessages.GetMessage(BotMessageKey.AllWordsLearned);
            await telegramBotService.SendMessageAsync(completionMessage, chatId);
            return;
        }

        // Send new words
        var message = messageFormatterService.FormatNewWords(words);
        await telegramBotService.SendMessageAsync(message, chatId, ParseMode.Html);

        // Mark as learned
        await wordService.MarkWordsAsLearnedAsync(chatId, words.Select(w => w.Id));

        logger.LogInformation("New words completed for chat {ChatId}. Words learned: {Count}", chatId, words.Count);
    }
}