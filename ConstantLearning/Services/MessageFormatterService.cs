using ConstantLearning.Data.Entities;
using ConstantLearning.Enums;
using System.Text;

namespace ConstantLearning.Services;

public interface IMessageFormatterService
{
    string FormatRepetitionQuestions(List<Word> words);
    string FormatRepetitionAnswers(List<Word> words);
    string FormatNewWords(List<Word> words);
}

public class MessageFormatterService(
    IBotMessagesService botMessages,
    ITemplateService templateService) : IMessageFormatterService
{
    public string FormatRepetitionQuestions(List<Word> words)
    {
        if (words.Count == 0)
        {
            return botMessages.GetMessage(BotMessageKey.NoRepetitionWords);
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📚 {botMessages.GetMessage(BotMessageKey.RepetitionHeader)}");
        sb.AppendLine();

        for (var i = 0; i < words.Count; i++)
        {
            sb.AppendLine($"{i + 1}. {words[i].SourceMeaning}");
        }

        return sb.ToString();
    }

    public string FormatRepetitionAnswers(List<Word> words)
    {
        if (words.Count == 0)
        {
            return string.Empty;
        }

        var header = botMessages.GetMessage(BotMessageKey.AnswersHeader);
        var rows = BuildRepetitionAnswersRows(words);
        
        return templateService.Render(TemplateType.RepetitionAnswers, header, rows);
    }

    public string FormatNewWords(List<Word> words)
    {
        if (words.Count == 0)
        {
            return botMessages.GetMessage(BotMessageKey.NoNewWords);
        }

        var header = botMessages.GetMessage(BotMessageKey.NewWordsHeader);
        var rows = BuildNewWordsRows(words);
        
        return templateService.Render(TemplateType.NewWords, header, rows);
    }

    private static string BuildRepetitionAnswersRows(List<Word> words)
    {
        var maxSourceLength = words.Max(w => w.SourceMeaning.Length);
        var maxTargetLength = words.Max(w => w.TargetWord.Length);

        var sb = new StringBuilder();
        for (var i = 0; i < words.Count; i++)
        {
            var word = words[i];
            var num = $"{i + 1}.".PadRight(3);
            var source = word.SourceMeaning.PadRight(maxSourceLength);
            var target = word.TargetWord.PadRight(maxTargetLength);

            sb.AppendLine($"{num} {source}  →  {target}  [{word.PhoneticTranscription}]");
        }

        return sb.ToString().TrimEnd();
    }

    private static string BuildNewWordsRows(List<Word> words)
    {
        var maxTargetLength = words.Max(w => w.TargetWord.Length);
        var maxTranscriptionLength = words.Max(w => w.PhoneticTranscription.Length);

        var sb = new StringBuilder();
        for (var i = 0; i < words.Count; i++)
        {
            var word = words[i];
            var num = $"{i + 1}.".PadRight(3);
            var target = word.TargetWord.PadRight(maxTargetLength);
            var transcription = $"[{word.PhoneticTranscription}]".PadRight(maxTranscriptionLength + 2);

            sb.AppendLine($"{num} {target}  {transcription}  → {word.SourceMeaning}");
        }

        return sb.ToString().TrimEnd();
    }
}