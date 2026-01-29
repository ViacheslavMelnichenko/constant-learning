using ConstantLearning.Configuration;
using ConstantLearning.Data;
using ConstantLearning.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace ConstantLearning.Services;

public interface IWordImportService
{
    Task ImportFromCsvAsync();
}

public class WordImportService(
    AppDbContext context,
    ILogger<WordImportService> logger,
    IOptions<WordsImportOptions> importOptions)
    : IWordImportService
{
    private readonly WordsImportOptions _importOptions = importOptions.Value;

    public async Task ImportFromCsvAsync()
    {
        var filePath = _importOptions.CsvPath;
        
        if (!File.Exists(filePath))
        {
            logger.LogWarning("CSV file not found: {FilePath}", filePath);
            return;
        }

        var existingCount = await context.Words.CountAsync();
        if (existingCount > 0)
        {
            logger.LogInformation("Words already imported ({Count} words). Skipping import.", existingCount);
            return;
        }

        logger.LogInformation("Starting CSV import from {FilePath}", filePath);

        var lines = await File.ReadAllLinesAsync(filePath);
        var words = new List<Word>();

        // Skip header row
        for (var i = 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(',');
            if (parts.Length < 4)
            {
                logger.LogWarning("Invalid CSV line at row {Row}: {Line}", i + 1, line);
                continue;
            }

            words.Add(new Word
            {
                FrequencyRank = int.Parse(parts[0].Trim(), CultureInfo.InvariantCulture),
                TargetWord = parts[1].Trim(),
                SourceMeaning = parts[2].Trim(),
                PhoneticTranscription = parts[3].Trim(),
                ImportedAt = DateTime.UtcNow
            });
        }

        if (words.Count > 0)
        {
            await context.Words.AddRangeAsync(words);
            await context.SaveChangesAsync();
            logger.LogInformation("Successfully imported {Count} words", words.Count);
        }
        else
        {
            logger.LogWarning("No words found in CSV file");
        }
    }
}
