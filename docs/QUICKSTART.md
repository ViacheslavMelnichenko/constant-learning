﻿# Quick Start Guide

## Running with Docker Compose (Recommended)

### 1. Configure Environment

Create `.env` file:
```bash
cp .env.example .env
```

Edit `.env`:
```env
TELEGRAM_BOT_TOKEN=1234567890:ABCdefGHIjklMNOpqrsTUVwxyz
TELEGRAM_CHAT_ID=-1001234567890
```

### 2. Start Services

```bash
docker-compose up -d
```

### 3. Check Logs

```bash
docker-compose logs -f app
```

### 4. Stop Services

```bash
docker-compose down
```

## Getting Telegram Credentials

### Bot Token

1. Open Telegram and search for `@BotFather`
2. Send `/newbot`
3. Follow instructions
4. Copy the token (looks like `1234567890:ABCdefGHIjklMNOpqrsTUVwxyz`)

### Chat ID

1. Add your bot to a group chat
2. Send any message in the group
3. Visit: `https://api.telegram.org/bot<YOUR_BOT_TOKEN>/getUpdates`
4. Find `"chat":{"id":-1001234567890}` in the response
5. Copy the chat ID (negative number for groups)

## Customizing Language

Edit `docker-compose.yml` environment variables:

```yaml
Language__TargetLanguage: "English"
Language__SourceLanguage: "Ukrainian"
Language__TargetLanguageCode: "en"
Language__SourceLanguageCode: "uk"
```

## Changing Schedule

Edit `docker-compose.yml` cron expressions:

```yaml
Schedule__RepetitionCron: "0 0 9 * * ?"   # 9:00 AM daily
Schedule__NewWordsCron: "0 30 20 * * ?"    # 8:30 PM daily
```

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
