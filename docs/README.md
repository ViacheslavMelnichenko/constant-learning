﻿# Language Learning Telegram Bot

A production-ready, **language-agnostic** Telegram bot for learning foreign words through spaced repetition. Built with .NET and PostgreSQL, designed for Kubernetes deployment.

## Features

- ✅ **Generic language support** - Learn any language pair (Polish→Ukrainian by default, English next)
- ✅ **Spaced repetition** - Daily review of previously learned words
- ✅ **Frequency-based learning** - Words ordered by real-world usage frequency
- ✅ **Configurable schedules** - Separate cron schedules for repetition and new words
- ✅ **IOptions pattern** - All settings configurable via environment variables
- ✅ **Docker Compose** - Complete setup with PostgreSQL included
- ✅ **Kubernetes-ready** - Safe for single-replica deployments
- ✅ **Production-grade** - Clean architecture, comprehensive logging, error handling

## Quick Start with Docker Compose

### 1. Clone and Configure

```bash
cd C:\projects\constant-learning\ConstantLearning
cp .env.example .env
```

Edit `.env` with your Telegram credentials:
```env
TELEGRAM_BOT_TOKEN=your_bot_token_here
TELEGRAM_CHAT_ID=123456789
```

### 2. Run

```bash
docker-compose up -d
```

That's it! The bot will:
- Start PostgreSQL database
- Run migrations automatically
- Import words from CSV
- Begin scheduled learning flows

### 3. Check logs

```bash
docker-compose logs -f app
```

## Configuration

All settings use the **IOptions pattern** and can be configured via:
- `appsettings.json`
- Environment variables
- Docker Compose environment variables
- Kubernetes ConfigMaps/Secrets

### Configuration Sections

#### `Telegram`
```json
{
  "BotToken": "your_bot_token",
  "ChatId": 123456789
}
```
Environment variables: `Telegram__BotToken`, `Telegram__ChatId`

#### `Schedule`
Quartz cron format: `second minute hour day-of-month month day-of-week`
```json
{
  "RepetitionCron": "0 0 9 * * ?",
  "NewWordsCron": "0 0 20 * * ?"
}
```
Examples:
- `0 0 9 * * ?` - Every day at 9:00 AM
- `0 0 20 * * ?` - Every day at 8:00 PM
- `0 0 */3 * * ?` - Every 3 hours

#### `Learning`
```json
{
  "RepetitionWordsCount": 10,
  "NewWordsCount": 3,
  "AnswerDelaySeconds": 5
}
```

#### `Language`
**Generic language support** - configure any language pair:
```json
{
  "TargetLanguage": "Polish",
  "SourceLanguage": "Ukrainian",
  "TargetLanguageCode": "pl",
  "SourceLanguageCode": "uk"
}
```

For English learning:
```json
{
  "TargetLanguage": "English",
  "SourceLanguage": "Ukrainian",
  "TargetLanguageCode": "en",
  "SourceLanguageCode": "uk"
}
```

Supported source languages for UI messages: `uk` (Ukrainian), `en` (English)

#### `WordsImport`
```json
{
  "CsvPath": "/app/data/words.csv"
}
```

## Architecture

### Tech Stack
- **.NET 10.0** (ASP.NET Core)
- **PostgreSQL** with EF Core
- **Telegram.Bot** SDK
- **Quartz.NET** for scheduling
- Docker & Docker Compose
- Kubernetes-ready

### Components

1. **Configuration** (`Configuration/`)
   - `TelegramOptions`, `ScheduleOptions`, `LearningOptions`
   - `LanguageOptions`, `WordsImportOptions`

2. **Data Layer** (`Data/`)
   - `AppDbContext`: EF Core context
   - `Entities/`: Word, LearnedWord, BotConfiguration

3. **Services** (`Services/`)
   - `WordService`: Word selection and progress tracking
   - `TelegramBotService`: Telegram API integration and command handling
   - `MessageFormatterService`: Language-aware message formatting
   - `WordImportService`: CSV import
   - `ProgressService`: Progress management (restart functionality)

4. **Jobs** (`Jobs/`)
   - `RepetitionJob`: Daily repetition flow
   - `NewWordsJob`: Daily new words flow

5. **Controllers** (`Controllers/`)
   - `TelegramController`: Webhook endpoint

### Database Schema

#### `Words`
- `Id` (PK)
- `TargetWord` - Word in the language being learned
- `SourceMeaning` - Meaning in native language
- `PhoneticTranscription`
- `FrequencyRank` (indexed)
- `ImportedAt`

#### `LearnedWords`
- `Id` (PK)
- `WordId` (FK to Words, unique)
- `LearnedAt`
- `LastRepeatedAt`
- `RepetitionCount`

#### `BotConfigurations`
- `Id` (PK)
- `Key` (unique)
- `Value`
- `UpdatedAt`

## CSV Format

Create a CSV file with frequency-ordered words:

```csv
rank,target,source,transcription
1,be,бути,biː
2,and,і,ænd
3,of,з,ɒv
```

- **rank**: Frequency rank (1 = most common)
- **target**: Word in the target language (e.g., English, Polish)
- **source**: Meaning in source language (e.g., Ukrainian)
- **transcription**: Phonetic transcription (IPA or simplified)

## Local Development (without Docker)

### Prerequisites
- .NET 10.0 SDK
- PostgreSQL 16+

### Steps

1. Start PostgreSQL:
```powershell
docker run -d -p 5432:5432 -e POSTGRES_PASSWORD=postgres postgres:16-alpine
```

2. Update `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=constantlearning;Username=postgres;Password=postgres"
  },
  "Telegram": {
    "BotToken": "your_token",
    "ChatId": 123456789
  },
  "WordsImport": {
    "CsvPath": "words.csv"
  }
}
```

3. Create migration:
```powershell
dotnet ef migrations add InitialCreate
```

4. Run:
```powershell
dotnet run
```

## How It Works

### Daily Flow

**Repetition Flow**:
1. Selects N random previously learned words (configurable)
2. Sends meanings in source language (questions)
3. Waits X seconds (configurable)
4. Sends words in target language with transcriptions (answers)
5. Updates repetition statistics

**New Words Flow**:
1. Selects N next words by frequency rank (configurable)
2. Sends target word with source meaning and transcription
3. Marks words as learned for future repetition

### Word Selection Logic

- **Repetition**: Random selection from all learned words
- **New words**: Sequential by frequency rank (1→N)
- No duplicates: each word is learned once
- Shared progress for all group members

## API Endpoints

- `POST /api/telegram/webhook` - Telegram webhook
- `GET /api/telegram/health` - Health check

## Kubernetes Deployment

### ConfigMap Example

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: constantlearning-config
data:
  SCHEDULE__REPETITIONCRON: "0 0 9 * * ?"
  SCHEDULE__NEWWORDSCRON: "0 0 20 * * ?"
  LEARNING__REPETITIONWORDSCOUNT: "10"
  LEARNING__NEWWORDSCOUNT: "3"
  LEARNING__ANSWERDELAYSECONDS: "5"
  LANGUAGE__TARGETLANGUAGE: "English"
  LANGUAGE__SOURCELANGUAGE: "Ukrainian"
  LANGUAGE__TARGETLANGUAGECODE: "en"
  LANGUAGE__SOURCELANGUAGECODE: "uk"
  WORDSIMPORT__CSVPATH: "/app/data/words.csv"
```

### Secret Example

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: constantlearning-secret
type: Opaque
stringData:
  CONNECTION_STRING: "Host=postgres;Database=constantlearning;Username=user;Password=pass"
  TELEGRAM_BOT_TOKEN: "your-bot-token"
  TELEGRAM_CHAT_ID: "123456789"
```

### Deployment Example

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: constantlearning
spec:
  replicas: 1
  selector:
    matchLabels:
      app: constantlearning
  template:
    metadata:
      labels:
        app: constantlearning
    spec:
      containers:
      - name: app
        image: constantlearning:latest
        ports:
        - containerPort: 8080
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: constantlearning-secret
              key: CONNECTION_STRING
        - name: Telegram__BotToken
          valueFrom:
            secretKeyRef:
              name: constantlearning-secret
              key: TELEGRAM_BOT_TOKEN
        - name: Telegram__ChatId
          valueFrom:
            secretKeyRef:
              name: constantlearning-secret
              key: TELEGRAM_CHAT_ID
        envFrom:
        - configMapRef:
            name: constantlearning-config
        volumeMounts:
        - name: words-data
          mountPath: /app/data
      volumes:
      - name: words-data
        configMap:
          name: words-csv
```

## Production Considerations

- **Single replica**: Quartz jobs are safe for single instance
- **Database migrations**: Run automatically on startup
- **CSV import**: Runs once (checks if words already imported)
- **Idempotency**: Jobs can be safely re-run
- **Error handling**: Jobs log errors and continue on next schedule
- **Timezone**: Uses UTC, configure cron accordingly
- **Secrets**: Use Kubernetes secrets or environment variables for sensitive data

## Monitoring

Logs include:
- Job execution start/finish
- Word counts
- Import status
- Telegram API errors
- Database errors

Check application logs for:
```
"Starting repetition job"
"Repetition job completed successfully. Words repeated: {Count}"
"Starting new words job"
"New words job completed successfully. Words learned: {Count}"
```

## Adding a New Language

1. Update `Language` configuration:
```json
{
  "TargetLanguage": "Spanish",
  "SourceLanguage": "English",
  "TargetLanguageCode": "es",
  "SourceLanguageCode": "en"
}
```

2. Prepare CSV with words:
```csv
rank,target,source,transcription
1,ser,to be,seɾ
2,estar,to be (temporary),esˈtaɾ
```

3. Add localized messages in `MessageFormatterService.GetLocalizedMessage()` if needed

4. Restart the application - it will import the new words automatically


