using ConstantLearning.Configuration;
using ConstantLearning.Data;
using ConstantLearning.HostedServices;
using ConstantLearning.Jobs;
using ConstantLearning.Services;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Telegram.Bot;

namespace ConstantLearning;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure Options
        builder.Services.Configure<TelegramOptions>(builder.Configuration.GetSection(TelegramOptions.SectionName));
        builder.Services.Configure<ScheduleOptions>(builder.Configuration.GetSection(ScheduleOptions.SectionName));
        builder.Services.Configure<LearningOptions>(builder.Configuration.GetSection(LearningOptions.SectionName));
        builder.Services.Configure<WordsImportOptions>(
            builder.Configuration.GetSection(WordsImportOptions.SectionName));
        builder.Services.Configure<LanguageOptions>(builder.Configuration.GetSection(LanguageOptions.SectionName));

        // Database
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'DefaultConnection' not found");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Telegram Bot
        var telegramOptions = builder
                                  .Configuration
                                  .GetSection(TelegramOptions.SectionName)
                                  .Get<TelegramOptions>()
                              ?? throw new InvalidOperationException("Telegram configuration not found");

        builder.Services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(telegramOptions.BotToken));
        builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();

        // Services
        builder.Services.AddScoped<IWordService, WordService>();
        builder.Services.AddScoped<IMessageFormatterService, MessageFormatterService>();
        builder.Services.AddScoped<IWordImportService, WordImportService>();
        builder.Services.AddScoped<IProgressService, ProgressService>();
        builder.Services.AddScoped<IChatRegistrationService, ChatRegistrationService>();

        // Quartz Scheduler
        var scheduleOptions = builder
                                  .Configuration
                                  .GetSection(ScheduleOptions.SectionName)
                                  .Get<ScheduleOptions>()
                              ?? throw new InvalidOperationException("Schedule configuration not found");

        builder.Services.AddQuartz(q =>
        {
            // Repetition Job
            var repetitionJobKey = new JobKey("RepetitionJob");
            q.AddJob<RepetitionJob>(opts => opts.WithIdentity(repetitionJobKey));

            q.AddTrigger(opts => opts
                .ForJob(repetitionJobKey)
                .WithIdentity("RepetitionJob-trigger")
                .WithCronSchedule(scheduleOptions.RepetitionCron)
                .WithDescription("Repetition flow trigger"));

            // New Words Job
            var newWordsJobKey = new JobKey("NewWordsJob");
            q.AddJob<NewWordsJob>(opts => opts.WithIdentity(newWordsJobKey));

            q.AddTrigger(opts => opts
                .ForJob(newWordsJobKey)
                .WithIdentity("NewWordsJob-trigger")
                .WithCronSchedule(scheduleOptions.NewWordsCron)
                .WithDescription("New words flow trigger"));
        });

        builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

        // Hosted Services
        builder.Services.AddHostedService<DatabaseInitializationService>();

        // Controllers
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}