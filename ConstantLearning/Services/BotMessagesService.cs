namespace ConstantLearning.Services;

public interface IBotMessagesService
{
    string GetMessage(BotMessageKey key, params object[] args);
}

public class BotMessagesService : IBotMessagesService
{
    private readonly Dictionary<string, string> _messages;
    private readonly ILogger<BotMessagesService> _logger;

    public BotMessagesService(
        IConfiguration configuration,
        ILogger<BotMessagesService> logger)
    {
        _logger = logger;
        
        // Get language from configuration
        var language = configuration["Language:SourceLanguageCode"]?.ToLower() ?? "uk";
        
        // Load messages from JSON
        var messagesSection = configuration.GetSection($"BotMessages:{language}");
        _messages = messagesSection.GetChildren()
            .ToDictionary(x => x.Key, x => x.Value ?? string.Empty);

        if (_messages.Count == 0)
        {
            _logger.LogWarning("No bot messages loaded for language: {Language}", language);
        }
    }

    public string GetMessage(BotMessageKey key, params object[] args)
    {
        var keyString = key.ToString();
        
        if (_messages.TryGetValue(keyString, out var message))
        {
            return args.Length > 0 ? string.Format(message, args) : message;
        }

        _logger.LogWarning("Message key not found: {Key}", keyString);
        return $"[Message not found: {keyString}]";
    }
}
