so u# Quick Setup Guide

## 🚀 Get Started in 5 Minutes

### Step 1: Get a Bot Token

1. Open Telegram and find [@BotFather](https://t.me/BotFather)
2. Send `/newbot` and follow instructions
3. Save your bot token (looks like: `1234567890:ABCdef...`)

### Step 2: Clone & Configure

```bash
git clone https://github.com/ViacheslavMelnichenko/constant-learning.git
cd constant-learning
```

Edit `docker-compose.yml`:
```yaml
Telegram__BotToken: "YOUR_TOKEN_HERE"
```

### Step 3: Start

```bash
docker-compose up -d
```

### Step 4: Register Your Group

1. Add your bot to a Telegram group
2. Send in the group: `/startlearning`

**Done!** 🎉

The bot will now send:
- **09:00** - Repetition of learned words
- **20:00** - New words to learn

## 📝 Customize

### Change Schedule

```
/setrepetitiontime 08:00
/setnewwordstime 21:00
```

### Change Word Counts

```
/setwordscount 25
```
(2 new words, 5 repetition words)

### View All Commands

```
/help
```

## 🔧 Common Tasks

### View Logs
```bash
docker-compose logs -f app
```

### Restart Bot
```bash
docker-compose restart app
```

### Stop Bot
```bash
docker-compose down
```

### Change Language

1. Add your CSV file to `ConstantLearning/Resources/Words/`
2. Format: `words-{target}-{source}.csv`
3. Update `docker-compose.yml` language settings
4. Restart: `docker-compose restart app`

## 🆘 Troubleshooting

### Bot not responding?
```bash
docker-compose logs -f app | grep ERROR
```

### Wrong time zone?
Add to `docker-compose.yml`:
```yaml
TZ: "Europe/Kyiv"  # Your timezone
```

### Reset progress?
```
/restartprogress
```

## 📚 More Info

- [Full README](../README.md)
- [All Commands](COMMANDS.md)
- [GitHub Secrets Setup](GITHUB-SECRETS.md)
