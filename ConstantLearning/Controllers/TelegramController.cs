using ConstantLearning.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace ConstantLearning.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TelegramController(
    ITelegramBotService telegramBotService,
    ILogger<TelegramController> logger)
    : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> HandleWebhook([FromBody] Update update)
    {
        try
        {
            await telegramBotService.HandleUpdateAsync(update);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing webhook");
            return StatusCode(500);
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}