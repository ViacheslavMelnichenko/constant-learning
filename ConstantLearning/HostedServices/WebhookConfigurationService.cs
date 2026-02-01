using ConstantLearning.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace ConstantLearning.HostedServices;

public class WebhookConfigurationService(
    ITelegramBotClient botClient,
    IOptions<TelegramOptions> telegramOptions,
    ILogger<WebhookConfigurationService> logger)
    : IHostedService
{
    private readonly TelegramOptions _telegramOptions = telegramOptions.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var webhookUrl = _telegramOptions.WebhookUrl;
        
        if (string.IsNullOrEmpty(webhookUrl))
        {
            logger.LogWarning("Telegram:WebhookUrl is not configured. Webhook will not be set.");
            return;
        }

        const string requiredEndpoint = "/api/telegram/webhook";
        if (!webhookUrl.EndsWith(requiredEndpoint, StringComparison.OrdinalIgnoreCase))
        {
            webhookUrl = webhookUrl.TrimEnd('/') + requiredEndpoint;
            logger.LogInformation("WebhookUrl corrected to: {WebhookUrl}", webhookUrl);
        }


        try
        {
            await botClient.SetWebhook(
                url: webhookUrl,
                allowedUpdates: Array.Empty<UpdateType>(),
                cancellationToken: cancellationToken);
            
            logger.LogInformation("✅ Webhook set to: {WebhookUrl}", webhookUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set webhook to: {WebhookUrl}", webhookUrl);
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await botClient.DeleteWebhook(cancellationToken: cancellationToken);
            logger.LogInformation("Webhook deleted");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting webhook");
        }
    }
}
