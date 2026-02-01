using ConstantLearning.Data.Entities;
using ConstantLearning.Enums;
using ConstantLearning.Services;
using FluentAssertions;
using Moq;

namespace ConstantLearning.Tests.Services;

public class MessageFormatterServiceTests
{
    private readonly Mock<IBotMessagesService> _botMessagesMock;
    private readonly Mock<ITemplateService> _templateServiceMock;
    private readonly MessageFormatterService _service;

    public MessageFormatterServiceTests()
    {
        _botMessagesMock = new Mock<IBotMessagesService>();
        _templateServiceMock = new Mock<ITemplateService>();
        _service = new MessageFormatterService(_botMessagesMock.Object, _templateServiceMock.Object);

        // Setup default messages
        _botMessagesMock.Setup(x => x.GetMessage(BotMessageKey.NoRepetitionWords))
            .Returns("No words for repetition");
        _botMessagesMock.Setup(x => x.GetMessage(BotMessageKey.NoNewWords))
            .Returns("No new words");
        _botMessagesMock.Setup(x => x.GetMessage(BotMessageKey.RepetitionHeader))
            .Returns("Repetition");
        _botMessagesMock.Setup(x => x.GetMessage(BotMessageKey.AnswersHeader))
            .Returns("Answers");
        _botMessagesMock.Setup(x => x.GetMessage(BotMessageKey.NewWordsHeader))
            .Returns("New words");
    }

    [Fact]
    public void FormatRepetitionQuestions_ReturnsNoWordsMessage_WhenListIsEmpty()
    {
        // Arrange
        var words = new List<Word>();

        // Act
        var result = _service.FormatRepetitionQuestions(words);

        // Assert
        result.Should().Be("No words for repetition");
    }

    [Fact]
    public void FormatRepetitionQuestions_FormatsCorrectly_WithMultipleWords()
    {
        // Arrange
        var words = new List<Word>
        {
            new() { SourceMeaning = "так", TargetWord = "tak", PhoneticTranscription = "tak" },
            new() { SourceMeaning = "для", TargetWord = "dla", PhoneticTranscription = "dla" }
        };

        // Act
        var result = _service.FormatRepetitionQuestions(words);

        // Assert
        result.Should().Contain("📚 Repetition");
        result.Should().Contain("1. так");
        result.Should().Contain("2. для");
    }

    [Fact]
    public void FormatRepetitionQuestions_NumbersWordsCorrectly()
    {
        // Arrange
        var words = new List<Word>
        {
            new() { SourceMeaning = "один", TargetWord = "one", PhoneticTranscription = "wʌn" },
            new() { SourceMeaning = "два", TargetWord = "two", PhoneticTranscription = "tuː" },
            new() { SourceMeaning = "три", TargetWord = "three", PhoneticTranscription = "θriː" }
        };

        // Act
        var result = _service.FormatRepetitionQuestions(words);

        // Assert
        result.Should().MatchRegex(@"1\.\s+один");
        result.Should().MatchRegex(@"2\.\s+два");
        result.Should().MatchRegex(@"3\.\s+три");
    }

    [Fact]
    public void FormatRepetitionAnswers_ReturnsEmpty_WhenListIsEmpty()
    {
        // Arrange
        var words = new List<Word>();

        // Act
        var result = _service.FormatRepetitionAnswers(words);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FormatRepetitionAnswers_CallsTemplateService_WithCorrectParameters()
    {
        // Arrange
        var words = new List<Word>
        {
            new() { SourceMeaning = "так", TargetWord = "tak", PhoneticTranscription = "tak" }
        };
        _templateServiceMock.Setup(x => x.Render(It.IsAny<TemplateType>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("rendered");

        // Act
        var result = _service.FormatRepetitionAnswers(words);

        // Assert
        _templateServiceMock.Verify(x => x.Render(
            TemplateType.RepetitionAnswers,
            "Answers",
            It.IsAny<string>()), Times.Once);
        result.Should().Be("rendered");
    }

    [Fact]
    public void FormatNewWords_ReturnsNoWordsMessage_WhenListIsEmpty()
    {
        // Arrange
        var words = new List<Word>();

        // Act
        var result = _service.FormatNewWords(words);

        // Assert
        result.Should().Be("No new words");
    }

    [Fact]
    public void FormatNewWords_CallsTemplateService_WithCorrectParameters()
    {
        // Arrange
        var words = new List<Word>
        {
            new() { SourceMeaning = "так", TargetWord = "tak", PhoneticTranscription = "tak" }
        };
        _templateServiceMock.Setup(x => x.Render(It.IsAny<TemplateType>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("rendered");

        // Act
        var result = _service.FormatNewWords(words);

        // Assert
        _templateServiceMock.Verify(x => x.Render(
            TemplateType.NewWords,
            "New words",
            It.IsAny<string>()), Times.Once);
        result.Should().Be("rendered");
    }
}
