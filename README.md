# Telegram Language Learning Bot

A production-ready Telegram bot for learning foreign words through spaced repetition. Built with .NET 10, PostgreSQL, and EF Core.

## 🚀 Quick Start

### Prerequisites
- Docker Desktop
- Telegram bot token from [@BotFather](https://t.me/BotFather)

### Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/ViacheslavMelnichenko/constant-learning.git
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

- 🔄 **Multi-Chat Support** - Multiple groups with independent progress
- 🌍 **Language Agnostic** - Any language pair (Polish/Ukrainian by default)
- ⏰ **Per-Chat Scheduling** - Each group sets its own learning times
- 💾 **Progress Tracking** - Persistent learning state per group
- 🎯 **Spaced Repetition** - Automatic review of learned words
- 📚 **Frequency-Based** - Words ordered by real-world usage
- 🐳 **Docker Ready** - One command deployment

## 📖 Bot Commands

- `/start-learning` - Register group for learning
- `/stop-learning` - Pause scheduled messages
- `/restart-progress` - Clear learning progress
- `/set-repetition-time HH:MM` - Set daily repetition time (e.g., `09:00`)
- `/set-new-words-time HH:MM` - Set daily new words time (e.g., `20:00`)
- `/help` - Show all commands

📚 **[Full Commands Reference →](docs/COMMANDS.md)**

## 🔧 Configuration

### Basic Settings

Edit `docker-compose.yml`:

```yaml
# Telegram Bot
Telegram__BotToken: "YOUR_BOT_TOKEN"

# Learning Parameters
Learning__RepetitionWordsCount: "10"    # Words to repeat daily
Learning__NewWordsCount: "3"            # New words per day
Learning__AnswerDelaySeconds: "30"      # Delay before showing answers

# Language Settings (any language pair)
Language__TargetLanguage: "Polish"
Language__SourceLanguage: "Ukrainian"
Language__TargetLanguageCode: "pl"
Language__SourceLanguageCode: "uk"
```

### Changing Language

The bot automatically imports all CSV files from `Resources/Words/` folder.

To add a new language:
1. Create a CSV file: `words-{target}-{source}.csv` (e.g., `words-english-ukrainian.csv`)
2. Place it in `Resources/Words/` folder
3. Update language settings:

```yaml
Language__TargetLanguage: "English"
Language__TargetLanguageCode: "en"
Language__SourceLanguage: "Ukrainian"
Language__SourceLanguageCode: "uk"
```

**Note:** All CSV files in the folder will be imported on first startup.

### Per-Chat Schedules

Each group can set its own schedule:

```
/set-repetition-time 09:00
/set-new-words-time 20:00
```

Default times for new groups: 09:00 (repetition) and 20:00 (new words).

## 📊 How It Works

### Daily Flow

**Morning (09:00 by default)** - Repetition:
```
📚 Повторення — згадайте переклад:
1. бути
2. мати
...

[30 seconds later]

✅ Відповіді:
1.  бути      →  być  [być]
2.  мати      →  mieć [mjeć]
...
```

**Evening (20:00 by default)** - New Words:
```
🆕 Нові слова:

1.  tak   [tak]   → так
2.  dla   [dla]   → для
3.  więc  [vjenʦ] → отже
```

### Architecture

```
┌─────────────┐
│  Telegram   │
│   Groups    │
└──────┬──────┘
       │
       ▼
┌─────────────────┐
│  Telegram Bot   │
│   (Webhooks)    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐      ┌──────────────┐
│   Bot Service   │◄────►│  PostgreSQL  │
│   - Commands    │      │  - Words     │
│   - Formatting  │      │  - Progress  │
└────────┬────────┘      │  - Chats     │
         │               └──────────────┘
         ▼
┌─────────────────┐
│  Quartz Jobs    │
│  (Every minute) │
│  - Check times  │
│  - Send msgs    │
└─────────────────┘
```

## 🗄️ Database

- **Words** - Vocabulary with frequency ranking
- **LearnedWords** - Per-group learning progress
- **ChatRegistrations** - Active groups with schedules
- **BotConfigurations** - Runtime settings

## 🐳 Docker Deployment

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f app

# Restart bot
docker-compose restart app

# Stop all
docker-compose down
```

## 🌐 Production Deployment (Webhook)

For production, configure webhook instead of polling:

```yaml
# docker-compose.yml
Telegram__WebhookUrl: "https://yourdomain.com/api/telegram/webhook"
```

📚 **[Webhook Setup Guide →](docs/WEBHOOK-SETUP.md)**

## 🛠️ Development

### Requirements
- .NET 10.0 SDK
- PostgreSQL 16+
- Telegram Bot Token

### Local Run
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
├── Configuration/       # IOptions configuration
├── Data/               # EF Core entities
│   └── Entities/
├── Enums/              # Application enums
├── Services/           # Business logic
├── Jobs/               # Quartz scheduled jobs
├── Controllers/        # Telegram webhook endpoint
├── HostedServices/     # Background services
├── Resources/          # Localization & templates
│   ├── BotMessages.json
│   ├── Templates/      # HTML templates
│   └── Words/          # CSV vocabulary files
├── Migrations/         # Database migrations
├── docs/               # Documentation
└── docker-compose.yml  # Docker setup
```

## 📖 Documentation

- **[Quick Start Guide](docs/QUICKSTART.md)** - Detailed setup
- **[Commands Reference](docs/COMMANDS.md)** - All bot commands
- **[Webhook Setup](docs/WEBHOOK-SETUP.md)** - Production deployment

## 🤝 Contributing

Contributions welcome! Please submit a Pull Request.

## 📄 License

MIT License

---

**Built with ❤️ using .NET, PostgreSQL, and Telegram Bot API**
