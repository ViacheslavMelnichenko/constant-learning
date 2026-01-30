﻿﻿# Quick Start Guide

## Running with Docker Compose (Recommended)

### 1. Configure Bot Token

Edit `docker-compose.yml` and set your Telegram bot token:

```yaml
environment:
  Telegram__BotToken: "YOUR_BOT_TOKEN_HERE"
```

Or set environment variable:
```bash
export TELEGRAM_BOT_TOKEN="1234567890:ABCdefGHIjklMNOpqrsTUVwxyz"
```

### 2. Start Services

```bash
docker-compose up -d
```

### 3. Register Your Chat

Add the bot to your Telegram group and send:
```
/start-learning
```

### 4. Configure Schedule (Optional)

Set when you want to receive messages:
```
/set-repetition-time 09:00
/set-new-words-time 20:00
```

### 5. Check Logs

```bash
docker-compose logs -f app
```

### 6. Stop Services

```bash
docker-compose down
```

## Getting Telegram Credentials

### Bot Token

1. Open Telegram and search for `@BotFather`
2. Send `/newbot`
3. Follow instructions
4. Copy the token (looks like `1234567890:ABCdefGHIjklMNOpqrsTUVwxyz`)

**Note:** Chat ID is no longer needed! Each chat registers itself using `/start-learning` command.

## Customizing Language

Edit `docker-compose.yml` environment variables:

```yaml
Language__TargetLanguage: "English"
Language__SourceLanguage: "Ukrainian"
Language__TargetLanguageCode: "en"
Language__SourceLanguageCode: "uk"
```

## Changing Schedule

Each chat configures its own schedule using bot commands:

```
/set-repetition-time 09:00     # When to send repetition words
/set-new-words-time 20:00      # When to send new words
```

**Default times for new chats:**
- Repetition: 09:00 (local time)
- New words: 20:00 (local time)

**Note:** Schedule is now managed per-chat in the database. Each group can have different times!

## Customizing Word Counts

Edit `docker-compose.yml`:

```yaml
Learning__RepetitionWordsCount: "15"
Learning__NewWordsCount: "5"
Learning__AnswerDelaySeconds: "10"
```

## Viewing Database

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
