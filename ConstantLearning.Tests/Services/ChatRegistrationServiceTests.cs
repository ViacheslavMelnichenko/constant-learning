using ConstantLearning.Data;
using ConstantLearning.Data.Entities;
using ConstantLearning.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ConstantLearning.Tests.Services;

public class ChatRegistrationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<ILogger<ChatRegistrationService>> _loggerMock;
    private readonly ChatRegistrationService _service;

    public ChatRegistrationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _loggerMock = new Mock<ILogger<ChatRegistrationService>>();
        _service = new ChatRegistrationService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task IsChatRegisteredAsync_ReturnsFalse_WhenChatNotRegistered()
    {
        // Act
        var result = await _service.IsChatRegisteredAsync(12345);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsChatRegisteredAsync_ReturnsTrue_WhenChatIsActive()
    {
        // Arrange
        _context.ChatRegistrations.Add(new ChatRegistration
        {
            ChatId = 12345,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsChatRegisteredAsync(12345);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsChatRegisteredAsync_ReturnsFalse_WhenChatIsInactive()
    {
        // Arrange
        _context.ChatRegistrations.Add(new ChatRegistration
        {
            ChatId = 12345,
            IsActive = false
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.IsChatRegisteredAsync(12345);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RegisterChatAsync_CreatesNewRegistration_WhenChatNotExists()
    {
        // Act
        var result = await _service.RegisterChatAsync(12345, "Test Chat");

        // Assert
        result.Should().NotBeNull();
        result.ChatId.Should().Be(12345);
        result.ChatTitle.Should().Be("Test Chat");
        result.IsActive.Should().BeTrue();
        result.RepetitionTime.Should().Be("09:00");
        result.NewWordsTime.Should().Be("20:00");
        result.NewWordsCount.Should().Be(3);
        result.RepetitionWordsCount.Should().Be(10);

        var dbChat = await _context.ChatRegistrations.FirstOrDefaultAsync(c => c.ChatId == 12345);
        dbChat.Should().NotBeNull();
    }

    [Fact]
    public async Task RegisterChatAsync_ReactivatesChat_WhenChatExistsButInactive()
    {
        // Arrange
        _context.ChatRegistrations.Add(new ChatRegistration
        {
            ChatId = 12345,
            ChatTitle = "Old Title",
            IsActive = false
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RegisterChatAsync(12345, "New Title");

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeTrue();
        result.ChatTitle.Should().Be("New Title");

        var count = await _context.ChatRegistrations.CountAsync(c => c.ChatId == 12345);
        count.Should().Be(1);
    }

    [Fact]
    public async Task RegisterChatAsync_ReturnsExisting_WhenChatAlreadyActive()
    {
        // Arrange
        var existing = new ChatRegistration
        {
            ChatId = 12345,
            ChatTitle = "Existing Chat",
            IsActive = true
        };
        _context.ChatRegistrations.Add(existing);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.RegisterChatAsync(12345, "New Title");

        // Assert
        result.ChatId.Should().Be(12345);
        result.ChatTitle.Should().Be("Existing Chat");

        var count = await _context.ChatRegistrations.CountAsync(c => c.ChatId == 12345);
        count.Should().Be(1);
    }

    [Fact]
    public async Task GetAllActiveChatIdsAsync_ReturnsOnlyActiveChats()
    {
        // Arrange
        _context.ChatRegistrations.AddRange(
            new ChatRegistration { ChatId = 1, IsActive = true },
            new ChatRegistration { ChatId = 2, IsActive = false },
            new ChatRegistration { ChatId = 3, IsActive = true }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllActiveChatIdsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(new[] { 1L, 3L });
    }

    [Fact]
    public async Task DeactivateChatAsync_SetsIsActiveToFalse()
    {
        // Arrange
        _context.ChatRegistrations.Add(new ChatRegistration
        {
            ChatId = 12345,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        // Act
        await _service.DeactivateChatAsync(12345);

        // Assert
        var chat = await _context.ChatRegistrations.FirstOrDefaultAsync(c => c.ChatId == 12345);
        chat.Should().NotBeNull();
        chat!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateRepetitionTimeAsync_UpdatesTime_WhenChatIsActive()
    {
        // Arrange
        _context.ChatRegistrations.Add(new ChatRegistration
        {
            ChatId = 12345,
            IsActive = true,
            RepetitionTime = "09:00"
        });
        await _context.SaveChangesAsync();

        // Act
        await _service.UpdateRepetitionTimeAsync(12345, "10:30");

        // Assert
        var chat = await _context.ChatRegistrations.FirstOrDefaultAsync(c => c.ChatId == 12345);
        chat!.RepetitionTime.Should().Be("10:30");
    }

    [Fact]
    public async Task UpdateRepetitionTimeAsync_ThrowsException_WhenChatNotFound()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _service.UpdateRepetitionTimeAsync(12345, "10:30"))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not registered*");
    }

    [Fact]
    public async Task UpdateNewWordsTimeAsync_UpdatesTime_WhenChatIsActive()
    {
        // Arrange
        _context.ChatRegistrations.Add(new ChatRegistration
        {
            ChatId = 12345,
            IsActive = true,
            NewWordsTime = "20:00"
        });
        await _context.SaveChangesAsync();

        // Act
        await _service.UpdateNewWordsTimeAsync(12345, "21:00");

        // Assert
        var chat = await _context.ChatRegistrations.FirstOrDefaultAsync(c => c.ChatId == 12345);
        chat!.NewWordsTime.Should().Be("21:00");
    }

    [Fact]
    public async Task UpdateWordsCountAsync_UpdatesCounts_WhenChatIsActive()
    {
        // Arrange
        _context.ChatRegistrations.Add(new ChatRegistration
        {
            ChatId = 12345,
            IsActive = true,
            NewWordsCount = 3,
            RepetitionWordsCount = 10
        });
        await _context.SaveChangesAsync();

        // Act
        await _service.UpdateWordsCountAsync(12345, 5, 8);

        // Assert
        var chat = await _context.ChatRegistrations.FirstOrDefaultAsync(c => c.ChatId == 12345);
        chat!.NewWordsCount.Should().Be(5);
        chat.RepetitionWordsCount.Should().Be(8);
    }

    [Fact]
    public async Task UpdateWordsCountAsync_ThrowsException_WhenChatNotFound()
    {
        // Act & Assert
        await FluentActions.Invoking(() => _service.UpdateWordsCountAsync(12345, 5, 8))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not registered*");
    }

    [Fact]
    public async Task GetChatRegistrationAsync_ReturnsChat_WhenActive()
    {
        // Arrange
        _context.ChatRegistrations.Add(new ChatRegistration
        {
            ChatId = 12345,
            IsActive = true,
            ChatTitle = "Test Chat"
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetChatRegistrationAsync(12345);

        // Assert
        result.Should().NotBeNull();
        result!.ChatId.Should().Be(12345);
        result.ChatTitle.Should().Be("Test Chat");
    }

    [Fact]
    public async Task GetChatRegistrationAsync_ReturnsNull_WhenChatInactive()
    {
        // Arrange
        _context.ChatRegistrations.Add(new ChatRegistration
        {
            ChatId = 12345,
            IsActive = false
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetChatRegistrationAsync(12345);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
