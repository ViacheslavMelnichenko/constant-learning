using ConstantLearning.Configuration;
using ConstantLearning.Services;
using Microsoft.Extensions.Options;
using Quartz;

namespace ConstantLearning.Jobs;

[DisallowConcurrentExecution]
public class RepetitionJob(
    IWordService wordService,
    ITelegramBotService telegramBotService,
    IMessageFormatterService messageFormatterService,
    ILogger<RepetitionJob> logger,
    IOptions<LearningOptions> learningOptions,
    IServiceProvider serviceProvider)
    : IJob
{
    private readonly LearningOptions _learningOptions = learningOptions.Value;

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            logger.LogInformation("Starting repetition job for all registered chats");

            // Get all active chats
            using var scope = serviceProvider.CreateScope();
            var chatRegistrationService = scope.ServiceProvider.GetRequiredService<IChatRegistrationService>();
            var activeChatIds = await chatRegistrationService.GetAllActiveChatIdsAsync();

            if (activeChatIds.Count == 0)
            {
                logger.LogInformation("No registered chats found. Skipping repetition job.");
                return;
            }

            logger.LogInformation("Processing repetition for {Count} registered chat(s)", activeChatIds.Count);

            foreach (var chatId in activeChatIds)
            {
                try
                {
                    await ProcessRepetitionForChatAsync(chatId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error processing repetition for chat {ChatId}", chatId);
                    // Continue with other chats
                }
            }

            logger.LogInformation("Repetition job completed for all chats");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing repetition job");
            throw;
        }
    }

    private async Task ProcessRepetitionForChatAsync(long chatId)
    {
        logger.LogInformation("Starting repetition for chat {ChatId}", chatId);

        var words = await wordService.GetRandomLearnedWordsAsync(chatId, _learningOptions.RepetitionWordsCount);

        if (words.Count == 0)
        {
            logger.LogInformation("No learned words available for repetition in chat {ChatId}", chatId);
            return;
        }

        // Send questions (source language only)
        var questionsMessage = messageFormatterService.FormatRepetitionQuestions(words);
        await telegramBotService.SendMessageAsync(questionsMessage, chatId);

        // Wait before showing answers
        await Task.Delay(TimeSpan.FromSeconds(_learningOptions.AnswerDelaySeconds));

        // Send answers (target language + transcription)
        var answersMessage = messageFormatterService.FormatRepetitionAnswers(words);
        await telegramBotService.SendMessageAsync(answersMessage, chatId);

        // Update repetition stats
        await wordService.UpdateRepetitionAsync(chatId, words.Select(w => w.Id));

        logger.LogInformation("Repetition completed for chat {ChatId}. Words repeated: {Count}", chatId, words.Count);
    }
}