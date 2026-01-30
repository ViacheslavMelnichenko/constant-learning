# Schedule Configuration Guide

## Overview

Each Telegram chat can configure its own learning schedule using bot commands. This allows different groups to have messages sent at times that work best for them.

---

## Quick Start

### Set Repetition Time
```
/set-repetition-time 09:00
```

### Set New Words Time
```
/set-new-words-time 20:00
```

---

## How It Works

### 1. Default Times

When a chat registers with `/start-learning`, it gets default schedule times:
- **Repetition**: 09:00 (morning review)
- **New words**: 20:00 (evening learning)

### 2. Per-Chat Configuration

Each chat stores its own schedule in the database:
- Independent of other chats
- No need to restart the application
- Changes take effect immediately (next minute)

### 3. Job Execution

Background jobs run **every minute** and:
1. Check all registered chats
2. Compare current time with each chat's configured time
3. Send messages only to chats with matching times
4. Log how many chats were processed

---

## Commands

### `/set-repetition-time HH:MM`

Set the time when repetition messages are sent.

**Format**: 24-hour format (HH:MM)

**Examples**:
```
/set-repetition-time 09:00    ✅ 9:00 AM
/set-repetition-time 14:30    ✅ 2:30 PM
/set-repetition-time 07:45    ✅ 7:45 AM
/set-repetition-time 23:00    ✅ 11:00 PM
```

**Invalid examples**:
```
/set-repetition-time 9:00     ❌ Missing leading zero
/set-repetition-time 25:00    ❌ Invalid hour
/set-repetition-time abc      ❌ Not a time
/set-repetition-time          ❌ Missing time
```

**Success response (Ukrainian)**:
```
✅ Час повторення встановлено на *09:00*

Повторення буде надсилатися щодня о цій годині.
```

**Success response (English)**:
```
✅ Repetition time set to *09:00*

Repetitions will be sent daily at this time.
```

---

### `/set-new-words-time HH:MM`

Set the time when new word messages are sent.

**Format**: 24-hour format (HH:MM)

**Examples**:
```
/set-new-words-time 20:00     ✅ 8:00 PM
/set-new-words-time 18:30     ✅ 6:30 PM
/set-new-words-time 12:00     ✅ Noon
/set-new-words-time 00:00     ✅ Midnight
```

**Success response (Ukrainian)**:
```
✅ Час нових слів встановлено на *20:00*

Нові слова будуть надсилатися щодня о цій годині.
```

**Success response (English)**:
```
✅ New words time set to *20:00*

New words will be sent daily at this time.
```

---

## Use Cases

### Morning Learner
```
/set-repetition-time 07:00
/set-new-words-time 08:00
```
Review before work, learn new words during breakfast.

### Evening Learner
```
/set-repetition-time 19:00
/set-new-words-time 20:30
```
Study after work/dinner.

### Lunch Break Learner
```
/set-repetition-time 12:00
/set-new-words-time 12:30
```
Use lunch break for learning.

### Night Owl
```
/set-repetition-time 22:00
/set-new-words-time 23:00
```
Study before bed.

### Weekend-Focused Group
Different groups can have completely different schedules:
- **Group A**: 09:00 and 20:00 (default)
- **Group B**: 07:30 and 21:30 (early bird + night owl)
- **Group C**: 12:00 and 18:00 (lunch + evening)

---

## Database Structure

### ChatRegistrations Table

```sql
CREATE TABLE "ChatRegistrations" (
    "Id" SERIAL PRIMARY KEY,
    "ChatId" BIGINT UNIQUE NOT NULL,
    "ChatTitle" VARCHAR(200),
    "RegisteredAt" TIMESTAMP NOT NULL,
    "IsActive" BOOLEAN NOT NULL,
    "RepetitionTime" VARCHAR(5) NOT NULL DEFAULT '09:00',
    "NewWordsTime" VARCHAR(5) NOT NULL DEFAULT '20:00'
);
```

### Check Current Schedule

```sql
SELECT "ChatId", "ChatTitle", "RepetitionTime", "NewWordsTime", "IsActive"
FROM "ChatRegistrations"
WHERE "IsActive" = true
ORDER BY "ChatId";
```

Example output:
```
 ChatId      | ChatTitle        | RepetitionTime | NewWordsTime | IsActive
-------------+------------------+----------------+--------------+----------
 -1001234567 | My Learning Chat | 09:30          | 20:00        | true
 -1007654321 | Study Group      | 08:00          | 19:00        | true
 -1009876543 | Language Team    | 12:00          | 18:00        | true
```

### Update Schedule Directly (if needed)

```sql
-- Update repetition time
UPDATE "ChatRegistrations"
SET "RepetitionTime" = '10:00'
WHERE "ChatId" = -1001234567;

-- Update new words time
UPDATE "ChatRegistrations"
SET "NewWordsTime" = '21:00'
WHERE "ChatId" = -1001234567;
```

---

## Technical Details

### Job Execution Flow

#### Every Minute (Quartz Cron: `0 * * * * ?`)

1. **RepetitionJob** and **NewWordsJob** wake up
2. Get current time in HH:MM format
3. Query all active chat registrations
4. For each chat:
   - Parse chat's configured time
   - Compare with current time
   - If match → execute job for that chat
   - If no match → skip
5. Log results

#### Example Execution Log

```
[09:00:00 INF] Starting repetition job check for all registered chats
[09:00:00 INF] Current time: 09:00. Checking 5 chat(s) for scheduled repetition
[09:00:01 INF] Starting repetition for chat -1001234567
[09:00:02 INF] Repetition completed for chat -1001234567. Words repeated: 10
[09:00:02 INF] Starting repetition for chat -1009876543
[09:00:03 INF] Repetition completed for chat -1009876543. Words repeated: 10
[09:00:03 INF] Repetition job check completed. Processed 2 chat(s)
```

### Time Zone Considerations

**Current Implementation**: Uses server local time

**Example**:
- Server in UTC timezone: `/set-repetition-time 09:00` → 09:00 UTC
- Server in Europe/Warsaw: `/set-repetition-time 09:00` → 09:00 Poland time
- Server in America/New_York: `/set-repetition-time 09:00` → 09:00 EST/EDT

**Best Practice**: 
- Run server in your local timezone
- Or document which timezone the bot uses
- Future enhancement: Store timezone per chat

---

## Error Handling

### Invalid Format

**Command**: `/set-repetition-time 9:00`

**Response**:
```
❌ Невірний формат часу. Використовуйте HH:MM (наприклад, 09:30)
```

### Missing Time

**Command**: `/set-repetition-time`

**Response**:
```
❌ Невірний формат команди.

Використання: `/set-repetition-time HH:MM`
Приклад: `/set-repetition-time 09:30`
```

### Invalid Hour/Minute

**Command**: `/set-repetition-time 25:00` or `/set-repetition-time 09:60`

**Response**:
```
❌ Невірний формат часу. Використовуйте HH:MM (наприклад, 09:30)
```

### Chat Not Registered

If chat is not registered (hasn't used `/start-learning`):

**Response**:
```
❌ Помилка при оновленні часу. Спробуйте пізніше.
```

**Log**:
```
[ERROR] Chat -1001234567 is not registered or not active
```

**Solution**: First use `/start-learning` to register the chat.

---

## Migration from Old System

### Old System (Before)
- Schedule configured via environment variables
- Same schedule for all chats
- Required app restart to change
- Cron format: `"0 0 9 * * ?"`

### New System (Now)
- Schedule configured per-chat via bot commands
- Each chat has independent schedule
- Changes effective immediately
- Simple format: `"09:00"`

### Migrating Existing Deployment

1. **Database migration runs automatically**
   - Adds `RepetitionTime` and `NewWordsTime` columns
   - Sets default values: `09:00` and `20:00`

2. **Existing chats get defaults**
   - All registered chats automatically get `09:00` and `20:00`
   - No action needed

3. **Environment variables ignored**
   - `Schedule__RepetitionCron` → Not used
   - `Schedule__NewWordsCron` → Not used
   - Can be removed from docker-compose.yml (optional)

4. **Jobs run every minute**
   - Old: Jobs ran at configured cron times
   - New: Jobs run every minute, check each chat's time

---

## Troubleshooting

### Messages not sent at configured time

**Check 1**: Verify chat is registered
```sql
SELECT * FROM "ChatRegistrations" WHERE "ChatId" = -1001234567;
```

**Check 2**: Verify schedule times
```sql
SELECT "ChatId", "RepetitionTime", "NewWordsTime" 
FROM "ChatRegistrations" 
WHERE "ChatId" = -1001234567;
```

**Check 3**: Check job logs
```bash
docker-compose logs app | grep -i "repetition\|new words"
```

**Check 4**: Verify current server time
```bash
docker exec -it constantlearning-app date +%H:%M
```

### Time doesn't match expected

**Issue**: Server timezone might be different than expected

**Check server timezone**:
```bash
docker exec -it constantlearning-app date +%Z
```

**Solution**: Either:
1. Adjust times to match server timezone
2. Change server timezone (in Dockerfile)
3. Wait for future timezone support feature

---

## Future Enhancements

Potential improvements:

1. **Timezone Support**
   - Store timezone per chat
   - Convert times to server timezone
   - Show time in chat's timezone

2. **View Current Schedule**
   - `/show-schedule` command
   - Display current repetition and new words times

3. **Multiple Daily Sessions**
   - Allow multiple time slots per day
   - Morning + evening repetition

4. **Day-Specific Schedules**
   - Different times for weekdays/weekends
   - Skip weekends option

5. **Pause/Resume**
   - Temporarily disable schedule
   - Resume without losing configuration

6. **Schedule Validation**
   - Warn if times are too close together
   - Suggest optimal spacing

---

## Summary

✅ **Simple commands** - Just `/set-repetition-time HH:MM`  
✅ **Per-chat configuration** - Each group independent  
✅ **Immediate effect** - Changes work next minute  
✅ **No restart needed** - All stored in database  
✅ **Multi-chat support** - Unlimited groups, different schedules  
✅ **User-friendly** - Clear error messages and validation  

---

*Last updated: 2026-01-30*
