# Telegram Language Learning Bot

A production-ready Telegram bot for learning foreign words through spaced repetition. Built with .NET 10, PostgreSQL, and EF Core.

## 🚀 Quick Start

### Prerequisites
- Docker Desktop
- Telegram bot token from [@BotFather](https://t.me/BotFather)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/YOUR_USERNAME/constant-learning.git
   cd constant-learning
   ```

2. **Configure the bot**
   
   Edit `docker-compose.yml` and set your bot token:
   ```yaml
   Telegram__BotToken: "YOUR_BOT_TOKEN_HERE"
   ```

3. **Start the services**
   ```bash
   docker-compose up -d
   ```

4. **Register your group**
   
   Add the bot to your Telegram group and send:
   ```
   /start-learning
   ```

That's it! The bot will now send scheduled word lessons to your group.

## ✨ Features

### Core Features
- 🔄 **Dynamic Chat Registration** - Any group can start learning with `/start-learning`
- 📊 **Multi-Chat Support** - Independent progress tracking per group
- 🌍 **Language Agnostic** - Support any language pair (Polish/Ukrainian by default)
- ⏰ **Scheduled Learning** - Automatic daily word repetition and new words
- 💾 **Progress Tracking** - Each group's learning progress is saved
- 🎯 **Spaced Repetition** - Review previously learned words
- 📚 **Frequency-Based** - Words ordered by real-world usage

### Bot Commands
- `/start-learning` - Register group for learning
- `/stop-learning` - Pause scheduled messages
- `/restart-progress` - Clear group's learning progress
- `/help` - Show available commands

### Technical Features
- IOptions pattern for configuration
- PostgreSQL with EF Core
- Quartz.NET for scheduling
- Docker Compose setup
- Automatic database migrations
- Comprehensive logging
- Health check endpoints

## 📖 Documentation

Detailed documentation is available in the [`docs/`](docs/) folder:

- **[Quick Start Guide](docs/QUICKSTART.md)** - Step-by-step setup
- **[Start Learning Command](docs/START-LEARNING.md)** - How dynamic registration works
- **[Multi-Chat Support](docs/MULTI-CHAT.md)** - Understanding per-group progress
- **[Commands Reference](docs/COMMANDS.md)** - All bot commands
- **[Complete Documentation](docs/README.md)** - Architecture and deployment

## 🔧 Configuration

### Language Settings

Switch to any language pair by editing `docker-compose.yml`:

```yaml
# For English learning:
Language__TargetLanguage: "English"
Language__SourceLanguage: "Ukrainian"
Language__TargetLanguageCode: "en"
Language__SourceLanguageCode: "uk"
WordsImport__CsvPath: "/app/data/words-english.csv"
```

### Schedule Configuration

Customize when messages are sent (times in UTC):

```yaml
Schedule__RepetitionCron: "0 0 9 * * ?"   # 9 AM daily
Schedule__NewWordsCron: "0 0 20 * * ?"    # 8 PM daily
```

### Learning Parameters

```yaml
Learning__RepetitionWordsCount: "10"      # Words to repeat
Learning__NewWordsCount: "3"              # New words per day
Learning__AnswerDelaySeconds: "5"         # Delay before showing answers
```

## 🗄️ Database Schema

- **Words** - Frequency-ordered vocabulary with transcriptions
- **LearnedWords** - Per-group learning progress (ChatId-based)
- **ChatRegistrations** - Active learning groups
- **BotConfigurations** - Runtime settings

## 📊 Example Usage

### Day 1: Start Learning
```
User: /start-learning

Bot: ✅ Навчання розпочато!
     Група "Study Group" успішно зареєстрована.
```

### Daily at 9 AM: Repetition
```
Bot: 📚 Повторення — згадайте переклад:
     1. бути
     2. мати
     ...
     
     [5 seconds later]
     
     ✅ Відповіді:
     1. бути → być [być]
     2. мати → mieć [mjeć]
     ...
```

### Daily at 8 PM: New Words
```
Bot: 🆕 Нові слова:
     1. tak [tak]
        так
     
     2. dla [dla]
        для
     ...
```

## 🐳 Docker Deployment

The project includes a complete Docker Compose setup:

```bash
# Start
docker-compose up -d

# View logs
docker-compose logs -f app

# Stop
docker-compose down
```

## 🚢 Kubernetes Deployment

See [Complete Documentation](docs/README.md) for Kubernetes manifests and production deployment guide.

## 🛠️ Development

### Requirements
- .NET 10.0 SDK
- PostgreSQL 16+
- Telegram Bot Token

### Local Development
```bash
cd ConstantLearning
dotnet restore
dotnet build
dotnet run
```

### Database Migrations
```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

## 📁 Project Structure

```
ConstantLearning/
├── Configuration/       # IOptions configuration classes
├── Data/               # EF Core entities and DbContext
├── Services/           # Business logic services
├── Jobs/               # Quartz scheduled jobs
├── Controllers/        # API controllers
├── Migrations/         # EF Core migrations
├── docs/               # Documentation
├── words.csv           # Polish/Ukrainian vocabulary
├── words-english.csv   # English/Ukrainian vocabulary
└── docker-compose.yml  # Docker setup
```

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License.

## 🙏 Acknowledgments

- Frequency word lists for vocabulary selection
- Telegram Bot API
- .NET and Entity Framework Core communities

## 📞 Support

For issues and questions:
- Check the [documentation](docs/)
- Open an issue on GitHub
- Review the [troubleshooting guide](docs/QUICKSTART.md#troubleshooting)

---

**Built with ❤️ using .NET 10, PostgreSQL, and Telegram Bot API**
