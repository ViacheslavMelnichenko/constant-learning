using ConstantLearning.Configuration;
using ConstantLearning.Data.Entities;
using Microsoft.Extensions.Options;
using System.Text;

namespace ConstantLearning.Services;

public interface IMessageFormatterService
{
    string FormatRepetitionQuestions(List<Word> words);
    string FormatRepetitionAnswers(List<Word> words);
    string FormatNewWords(List<Word> words);
}

public class MessageFormatterService(IOptions<LanguageOptions> languageOptions) : IMessageFormatterService
{
    private readonly LanguageOptions _languageOptions = languageOptions.Value;

    public string FormatRepetitionQuestions(List<Word> words)
    {
        if (words.Count == 0)
        {
            return GetLocalizedMessage("no_repetition_words");
        }

        var sb = new StringBuilder();
        sb.AppendLine($"📚 {GetLocalizedMessage("repetition_header")}");
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

        var sb = new StringBuilder();
        sb.AppendLine($"✅ {GetLocalizedMessage("answers_header")}");
        sb.AppendLine();

        for (var i = 0; i < words.Count; i++)
        {
            sb.AppendLine(
                $"{i + 1}. {words[i].SourceMeaning} → *{words[i].TargetWord}* [{words[i].PhoneticTranscription}]");
        }

        return sb.ToString();
    }

    public string FormatNewWords(List<Word> words)
    {
        if (words.Count == 0)
        {
            return GetLocalizedMessage("no_new_words");
        }

        var sb = new StringBuilder();
        sb.AppendLine($"🆕 {GetLocalizedMessage("new_words_header")}");
        sb.AppendLine();

        for (var i = 0; i < words.Count; i++)
        {
            sb.AppendLine($"{i + 1}. *{words[i].TargetWord}* [{words[i].PhoneticTranscription}]");
            sb.AppendLine($"   {words[i].SourceMeaning}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private string GetLocalizedMessage(string key)
    {
        // Simple localization based on source language
        // In future, this can be moved to resource files
        return _languageOptions.SourceLanguageCode.ToLower() switch
        {
            "uk" => key switch
            {
                "no_repetition_words" => "Немає слів для повторення.",
                "repetition_header" => "Повторення — згадайте переклад:",
                "answers_header" => "Відповіді:",
                "no_new_words" => "Немає нових слів для вивчення.",
                "new_words_header" => "Нові слова:",
                "all_words_learned" => "Всі слова вже вивчені! 🎉",
                _ => key
            },
            "en" => key switch
            {
                "no_repetition_words" => "No words for repetition.",
                "repetition_header" => "Repetition — recall the translation:",
                "answers_header" => "Answers:",
                "no_new_words" => "No new words to learn.",
                "new_words_header" => "New words:",
                "all_words_learned" => "All words already learned! 🎉",
                _ => key
            },
            _ => key
        };
    }
}