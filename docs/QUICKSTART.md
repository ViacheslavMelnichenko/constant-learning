# Quick Start Guide

Get your Telegram learning bot running in 5 minutes!

## Prerequisites

- Docker Desktop installed
- Telegram bot token from [@BotFather](https://t.me/BotFather)

## Setup

### 1. Get Bot Token

1. Open Telegram and search for `@BotFather`
2. Send `/newbot` and follow instructions
3. Copy your token (format: `1234567890:ABCdefGHIjklMNOpqrsTUVwxyz`)

### 2. Clone Repository

```bash
git clone https://github.com/ViacheslavMelnichenko/constant-learning.git
cd constant-learning
```

### 3. Configure Bot Token

Edit `docker-compose.yml`:

```yaml
environment:
  Telegram__BotToken: "YOUR_BOT_TOKEN_HERE"
```

Or use environment variable:

```bash
export TELEGRAM_BOT_TOKEN="1234567890:ABCdefGHIjklMNOpqrsTUVwxyz"
```

### 4. Start Services

```bash
docker-compose up -d
```

### 5. Register Your Group

1. Add the bot to your Telegram group
2. Send: `/start-learning`
3. Done! Messages will be sent at 09:00 and 20:00 daily

## Configuration

### Change Learning Times

Each group can set its own schedule:

```
/set-repetition-time 08:00
/set-new-words-time 19:00
```

### Change Word Counts

Edit `docker-compose.yml`:

```yaml
Learning__RepetitionWordsCount: "15"   # Words to repeat
Learning__NewWordsCount: "5"           # New words per day
Learning__AnswerDelaySeconds: "30"     # Delay before answers
```

### Change Language

To add/use different language:

1. Add CSV file to `Resources/Words/` (e.g., `words-english-ukrainian.csv`)
2. Update `docker-compose.yml`:

```yaml
Language__TargetLanguage: "English"
Language__TargetLanguageCode: "en"
```

All CSV files in `Resources/Words/` are automatically imported on first startup.

## Commands

| Command | Description |
|---------|-------------|
| `/start-learning` | Register group for learning |
| `/stop-learning` | Pause scheduled messages |
| `/restart-progress` | Clear learning progress |
| `/set-repetition-time HH:MM` | Set repetition time (e.g., `09:00`) |
| `/set-new-words-time HH:MM` | Set new words time (e.g., `20:00`) |
| `/help` | Show all commands |

📚 [Full Commands Reference →](COMMANDS.md)

## Useful Commands

```bash
# View logs
docker-compose logs -f app

# Restart bot
docker-compose restart app

# Stop everything
docker-compose down

# Check database
docker-compose exec postgres psql -U postgres -d constantlearning
```

## Troubleshooting

### Bot doesn't respond

1. Check logs:
   ```bash
   docker-compose logs -f app
   ```

2. Verify bot token is correct in `docker-compose.yml`

3. Ensure bot was added to the group

4. Send `/start-learning` to register the group

### Messages not sending

1. Check if group is registered:
   ```bash
   docker-compose logs app | grep "registered"
   ```

2. Verify schedule is set correctly:
   ```
   /set-repetition-time 09:00
   /set-new-words-time 20:00
   ```

3. Wait for scheduled time or check Quartz logs:
   ```bash
   docker-compose logs app | grep "Quartz"
   ```

### Database errors

1. Restart services:
   ```bash
   docker-compose down
   docker-compose up -d
   ```

2. Check database is running:
   ```bash
   docker-compose ps
   ```

3. Migrations run automatically on startup

## Next Steps

- 📖 [Commands Reference](COMMANDS.md) - Learn all bot commands
- 🌐 [Webhook Setup](WEBHOOK-SETUP.md) - Deploy to production
- 🔧 [docker-compose.yml](../docker-compose.yml) - See all configuration options

## Support

- 💬 Open an issue on GitHub
- 📚 Check [documentation](../README.md)
- 🔍 Search existing issues

---

**That's it!** Your bot is now running and ready to help you learn! 🎉

```bash
docker exec -it constantlearning-postgres psql -U postgres -d constantlearning
```

Check word count:
```sql
SELECT COUNT(*) FROM "Words";
SELECT COUNT(*) FROM "LearnedWords";
```

## Using Bot Commands

Once deployed, you can use these commands in your Telegram group:

### `/restart-progress`
Clears all learned words and starts from scratch. Useful if you want to:
- Begin learning again from the start
- Reset after testing
- Clear progress before adding new words

Example:
```
You: /restart-progress
Bot: ✅ Прогрес скинуто!
     Видалено 25 вивчених слів.
     Починаємо навчання спочатку! 🎯
```

### `/help` or `/start`
Shows available commands and bot information.

**Note:** Commands only work in the configured group chat (specified by ChatId in configuration).

## Troubleshooting

### Migration errors

```bash
docker-compose down -v
docker-compose up -d
```

### Check if bot is running

```bash
curl http://localhost:8080/api/telegram/health
```

### Force re-import words

```bash
docker exec -it constantlearning-postgres psql -U postgres -d constantlearning -c 'DELETE FROM "Words";'
docker-compose restart app
```

## Next Steps

1. Add more words to `words.csv`
2. Customize schedules
3. Deploy to Kubernetes (see README.md)
