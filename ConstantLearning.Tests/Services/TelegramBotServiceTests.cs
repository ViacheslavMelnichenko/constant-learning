using ConstantLearning.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Telegram.Bot;

namespace ConstantLearning.Tests.Services;

public class TelegramBotServiceTests
{
    [Fact]
    public void Constructor_InitializesService_Successfully()
    {
        // Arrange
        var botClientMock = new Mock<ITelegramBotClient>();
        var botMessagesMock = new Mock<IBotMessagesService>();
        var loggerMock = new Mock<ILogger<TelegramBotService>>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        // Act
        var service = new TelegramBotService(
            botClientMock.Object,
            botMessagesMock.Object,
            loggerMock.Object,
            serviceProviderMock.Object);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public async Task SendMessageAsync_DoesNotThrow_WithValidParameters()
    {
        // Arrange
        var botClientMock = new Mock<ITelegramBotClient>();
        var botMessagesMock = new Mock<IBotMessagesService>();
        var loggerMock = new Mock<ILogger<TelegramBotService>>();
        var serviceProviderMock = new Mock<IServiceProvider>();

        var service = new TelegramBotService(
            botClientMock.Object,
            botMessagesMock.Object,
            loggerMock.Object,
            serviceProviderMock.Object);

        const long chatId = 12345;
        const string text = "Test message";

        // Act
        Func<Task> act = async () => await service.SendMessageAsync(text, chatId);

        // Assert
        await act.Should().NotThrowAsync();
    }
}
