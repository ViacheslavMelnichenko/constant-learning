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

        // Auto-correct: append endpoint if missing
        const string requiredEndpoint = "/api/telegram/webhook";
        if (!webhookUrl.EndsWith(requiredEndpoint, StringComparison.OrdinalIgnoreCase))
        {
            webhookUrl = webhookUrl.TrimEnd('/') + requiredEndpoint;
            logger.LogInformation("WebhookUrl corrected to: {WebhookUrl}", webhookUrl);
        }

        // Validate URL
        if (!Uri.TryCreate(webhookUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https")
        {
            logger.LogError("Invalid webhook URL (must be HTTPS): {WebhookUrl}", webhookUrl);
            throw new InvalidOperationException($"Invalid webhook URL: {webhookUrl}");
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
