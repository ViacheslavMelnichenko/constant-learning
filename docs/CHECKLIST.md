# 🚀 Deployment Checklist

Use this checklist to ensure successful deployment.

## Pre-Deployment

### 1. Get Telegram Credentials

- [ ] Create bot via [@BotFather](https://t.me/BotFather)
  - Send `/newbot`
  - Follow instructions
  - Save bot token (format: `1234567890:ABCdefGHIjklMNOpqrsTUVwxyz`)

**Note:** Chat ID is no longer needed in configuration. Each chat registers itself using `/start-learning` command.

### 2. Prepare Environment

- [ ] Docker Desktop installed and running
- [ ] Bot token ready (will be added to user secrets or environment)

**Note:** Schedule times are now configured per-chat via bot commands after registration:
- `/set-repetition-time HH:MM` - Set when to send repetition words
- `/set-new-words-time HH:MM` - Set when to send new words

### 3. Prepare Word List

- [ ] Choose language:
  - `words.csv` for Polish/Ukrainian
  - `words-english.csv` for English/Ukrainian
  - Or create your own

- [ ] Verify CSV format:
  ```csv
  rank,target,source,transcription
  1,word,слово,wɜːrd
  ```

- [ ] Update `docker-compose.yml` volume mount if using custom CSV

## Deployment

### Option A: Use Deployment Script (Recommended)

```powershell
.\deploy.ps1
```

The script will:
- ✅ Check prerequisites
- ✅ Build and start services
- ✅ Verify deployment
- ✅ Show logs

### Option B: Manual Deployment

```bash
# 1. Stop existing containers
docker-compose down

# 2. Start services
docker-compose up -d --build

# 3. Check status
docker-compose ps

# 4. View logs
docker-compose logs -f app
```

## Post-Deployment Verification

### 1. Check Container Status

```bash
docker-compose ps
```

Expected output:
```
NAME                        STATUS
constantlearning-app        Up (healthy)
constantlearning-postgres   Up (healthy)
```

### 2. Check Logs

```bash
docker-compose logs app | Select-String -Pattern "migration|import|Starting"
```

Expected messages:
- ✅ "Running database migrations"
- ✅ "Successfully imported X words" (or "Words already imported")
- ✅ "Database initialization completed"
- ✅ "Application started"

### 3. Verify Database

```bash
docker exec -it constantlearning-postgres psql -U postgres -d constantlearning
```

In psql:
```sql
-- Check tables exist
\dt

-- Check word count
SELECT COUNT(*) FROM "Words";

-- Should return count from your CSV file
```

Exit with `\q`

### 4. Test Health Endpoint

```powershell
curl http://localhost:8080/api/telegram/health
```

Expected response:
```json
{"status":"healthy","timestamp":"2026-01-29T..."}
```

### 5. Test Telegram Bot Registration

In your Telegram group chat:

1. **Register the chat:**
   ```
   /start-learning
   ```
   
2. **Set schedule times (optional):**
   ```
   /set-repetition-time 09:00
   /set-new-words-time 20:00
   ```

3. **Wait for scheduled time** or check logs to see when jobs run

## Configuration Verification

### Check Environment Variables

```bash
docker exec -it constantlearning-app env | Select-String -Pattern "Telegram|Language|Learning"
```

Verify:
- [ ] `Telegram__BotToken` is set
- [ ] `Language__TargetLanguage` is correct
- [ ] `Learning__RepetitionWordsCount` is correct
- [ ] `Learning__NewWordsCount` is correct

**Note:** Schedule is now managed per-chat in the database, not via environment variables.
### Check CSV Import

```bash
docker exec -it constantlearning-app ls -la /app/data/
```

Should show:
```
-r--r--r-- 1 root root ... words.csv
```

## Troubleshooting

### Container won't start

```bash
# Check logs
docker-compose logs app

# Common issues:
# - Missing environment variables
# - Database connection failed
# - Invalid CSV path
```

### Database connection failed

```bash
# Check if PostgreSQL is healthy
docker-compose ps postgres

# Restart PostgreSQL
docker-compose restart postgres

# Wait a moment, then restart app
docker-compose restart app
```

### Words not imported

```bash
# Check import logs
docker-compose logs app | Select-String -Pattern "import"

# Verify CSV exists in container
docker exec -it constantlearning-app cat /app/data/words.csv | head -5

# Force re-import (deletes existing words)
docker exec -it constantlearning-postgres psql -U postgres -d constantlearning -c 'DELETE FROM "Words";'
docker-compose restart app
```

### Telegram messages not sent

- [ ] Verify bot token is correct
- [ ] Verify chat is registered (use `/start-learning` in the group)
- [ ] Check bot is added to the group
- [ ] Check bot has permission to send messages
- [ ] Review app logs for Telegram API errors
- [ ] Verify schedule times are set correctly (use bot commands to check/update)

### Schedule not working

- [ ] Verify chat is registered in the database
- [ ] Check schedule times for the chat:
  ```bash
  docker exec -it constantlearning-postgres psql -U postgres -d constantlearning -c 'SELECT "ChatId", "RepetitionTime", "NewWordsTime" FROM "ChatRegistrations";'
  ```
- [ ] Update schedule times using bot commands if needed
- [ ] Review job execution logs:
  ```bash
  docker-compose logs app | Select-String -Pattern "Quartz|Job|repetition|new words"
  ```
- [ ] Jobs run every minute and check each chat's configured times

## Production Deployment (Kubernetes)

### Before deploying to production:

- [ ] Review README.md Kubernetes section
- [ ] Create Kubernetes secrets
- [ ] Create ConfigMaps
- [ ] Update CSV with full word list (500-2000 words)
- [ ] Test in staging environment
- [ ] Set up monitoring and alerting
- [ ] Configure backup for PostgreSQL
- [ ] Set resource limits
- [ ] Configure persistent volumes

### Kubernetes Resources Needed:

- [ ] Secret for Telegram credentials
- [ ] Secret for database connection
- [ ] ConfigMap for application settings
- [ ] ConfigMap for words CSV
- [ ] Deployment for app
- [ ] StatefulSet or managed PostgreSQL
- [ ] Service for app
- [ ] Service for database
- [ ] PersistentVolumeClaim for database

## Monitoring

### Key Metrics to Watch:

- [ ] Container health status
- [ ] Database connections
- [ ] Job execution success rate
- [ ] Telegram API errors
- [ ] Memory usage
- [ ] CPU usage

### Important Log Patterns:

```bash
# Successful job execution
docker-compose logs app | Select-String -Pattern "completed successfully"

# Errors
docker-compose logs app | Select-String -Pattern "Error|Exception"

# Telegram issues
docker-compose logs app | Select-String -Pattern "Failed to send message"
```

## Rollback Procedure

If something goes wrong:

```bash
# 1. Stop current deployment
docker-compose down

# 2. Restore previous configuration
git checkout previous-commit

# 3. Restart
docker-compose up -d

# 4. Verify
docker-compose logs -f app
```

## Success Criteria

Deployment is successful when:

- [x] ✅ All containers running (postgres, app)
- [x] ✅ Health endpoint responds
- [x] ✅ Database migrations completed
- [x] ✅ Words imported
- [x] ✅ No errors in logs
- [x] ✅ Telegram bot responds (at scheduled time)

---

**Ready for production! 🎉**
