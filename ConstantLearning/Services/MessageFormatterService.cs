using ConstantLearning.Data.Entities;
using System.Text;

namespace ConstantLearning.Services;

public interface IMessageFormatterService
{
    string FormatRepetitionQuestions(List<Word> words);
    string FormatRepetitionAnswers(List<Word> words);
    string FormatNewWords(List<Word> words);
}

public class MessageFormatterService(IBotMessagesService botMessages) : IMessageFormatterService
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

        return BuildHtmlTable(
            header: botMessages.GetMessage(BotMessageKey.AnswersHeader),
            words: words,
            formatRow: (_, word, num, maxLengths) =>
            {
                var source = word.SourceMeaning.PadRight(maxLengths.sourceLength);
                var target = word.TargetWord.PadRight(maxLengths.targetLength);
                return $"{num} {source}  →  {target}  [{word.PhoneticTranscription}]";
            },
            calculateMaxLengths: wordList => (
                sourceLength: wordList.Max(w => w.SourceMeaning.Length),
                targetLength: wordList.Max(w => w.TargetWord.Length),
                transcriptionLength: 0
            )
        );
    }

    public string FormatNewWords(List<Word> words)
    {
        if (words.Count == 0)
        {
            return botMessages.GetMessage(BotMessageKey.NoNewWords);
        }

        return BuildHtmlTable(
            header: botMessages.GetMessage(BotMessageKey.NewWordsHeader),
            words: words,
            formatRow: (_, word, num, maxLengths) =>
            {
                var target = word.TargetWord.PadRight(maxLengths.targetLength);
                var transcription = $"[{word.PhoneticTranscription}]".PadRight(maxLengths.transcriptionLength + 2);
                return $"{num} {target}  {transcription}  → {word.SourceMeaning}";
            },
            calculateMaxLengths: wordList => (
                sourceLength: 0,
                targetLength: wordList.Max(w => w.TargetWord.Length),
                transcriptionLength: wordList.Max(w => w.PhoneticTranscription.Length)
            )
        );
    }

    private static string BuildHtmlTable(
        string header,
        List<Word> words,
        Func<int, Word, string, (int sourceLength, int targetLength, int transcriptionLength), string> formatRow,
        Func<List<Word>, (int sourceLength, int targetLength, int transcriptionLength)> calculateMaxLengths)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"✅ <b>{header}</b>\n");

        var maxLengths = calculateMaxLengths(words);

        sb.AppendLine("<pre>");
        for (var i = 0; i < words.Count; i++)
        {
            var word = words[i];
            var num = $"{i + 1}.".PadRight(3);
            sb.AppendLine(formatRow(i, word, num, maxLengths));
        }
        sb.AppendLine("</pre>");

        return sb.ToString();
    }
}

