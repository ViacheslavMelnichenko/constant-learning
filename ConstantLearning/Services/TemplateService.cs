using ConstantLearning.Enums;

namespace ConstantLearning.Services;

public interface ITemplateService
{
    string Render(TemplateType templateType, string header, string rows);
}

public class TemplateService : ITemplateService
{
    private readonly Dictionary<TemplateType, string> _templates = new()
    {
        { TemplateType.RepetitionAnswers, LoadTemplate(TemplateType.RepetitionAnswers) },
        { TemplateType.NewWords, LoadTemplate(TemplateType.NewWords) }
    };

    public string Render(TemplateType templateType, string header, string rows)
    {
        if (!_templates.TryGetValue(templateType, out var template))
        {
            throw new InvalidOperationException($"Template not found: {templateType}");
        }

        return template
            .Replace("{header}", header)
            .Replace("{rows}", rows);
    }

    private static string LoadTemplate(TemplateType templateType)
    {
        var fileName = GetTemplateFileName(templateType);
        var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Templates", fileName);
        
        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template file not found: {templatePath}");
        }

        return File.ReadAllText(templatePath);
    }

    private static string GetTemplateFileName(TemplateType templateType)
    {
        return templateType switch
        {
            TemplateType.RepetitionAnswers => "RepetitionAnswers.html",
            TemplateType.NewWords => "NewWords.html",
            _ => throw new ArgumentOutOfRangeException(nameof(templateType), templateType, "Unknown template type")
        };
    }
}
