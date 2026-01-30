﻿# Bot Commands Feature - Implementation Summary

## Feature Added: `/restart-progress` Command

### Overview
Users can now reset their learning progress by sending `/restart-progress` command in the Telegram group chat.

---

## What Was Implemented

### 1. ProgressService
**File:** `Services/ProgressService.cs`

New service to handle progress management:
```csharp
public interface IProgressService
{
    Task<int> RestartProgressAsync();
}
```

**Functionality:**
- Deletes all learned words from the database
- Returns count of removed words
- Logs the operation
- Allows starting learning from scratch

---

### 2. TelegramBotService Updates
**File:** `Services/TelegramBotService.cs`

**Added command handling:**
- `/restart-progress` - Clears all learned words
- `/help` - Shows available commands
- `/start` - Same as `/help`

**Features:**
- Localized responses (Ukrainian/English based on `SourceLanguageCode`)
- Error handling with user-friendly messages
- Logging for all command executions
- Scoped service resolution for database operations

---

### 3. Dependency Injection
**File:** `Program.cs`

Registered new service:
```csharp
builder.Services.AddScoped<IProgressService, ProgressService>();
```

---

## How It Works

### User Flow

1. **User sends command:**
   ```
   /restart-progress
   ```

2. **Bot processes:**
   - Validates command
   - Creates database scope
   - Calls `ProgressService.RestartProgressAsync()`
   - Deletes all `LearnedWords` records

3. **Bot responds (Ukrainian):**
   ```
   ✅ Прогрес скинуто!
   
   Видалено 25 вивчених слів.
   Починаємо навчання спочатку! 🎯
   ```

4. **Bot responds (English):**
   ```
   ✅ Progress restarted!
   
   Removed 25 learned words.
   Starting from scratch! 🎯
   ```

---

## Code Examples

### Sending the Command
In Telegram group chat:
```
/restart-progress
```

### Response Handling
The bot checks the configured source language and responds accordingly:

**Ukrainian UI (default):**
```
✅ Прогрес скинуто!
Видалено 25 вивчених слів.
Починаємо навчання спочатку! 🎯
```

**English UI:**
```
✅ Progress restarted!
Removed 25 learned words.
Starting from scratch! 🎯
```

### Help Command
```
/help
```

**Response (Ukrainian):**
```
📚 *Бот для вивчення мов*

🔹 Доступні команди:
• `/restart-progress` - Скинути прогрес навчання

📝 Бот автоматично надсилає:
• Повторення вивчених слів
• Нові слова для вивчення

Графік відправки налаштовано в конфігурації.
```

---

## Database Impact

### Before Command
```sql
SELECT COUNT(*) FROM "LearnedWords";
-- Returns: 25
```

### After `/restart-progress`
```sql
SELECT COUNT(*) FROM "LearnedWords";
-- Returns: 0
```

**Note:** The `Words` table is NOT affected - only learning progress is cleared.

---

## Security & Validation

### Chat ID Validation
Commands only work in the configured group chat:
- Bot checks incoming chat ID
- Compares with `Telegram:ChatId` configuration
- Ignores commands from other chats

### Error Handling
If database operation fails:
```
❌ Помилка при скиданні прогресу. Спробуйте пізніше.
```
or
```
❌ Error restarting progress. Please try again later.
```

---

## Use Cases

### 1. Reset After Testing
**Scenario:** You're testing the bot with sample words  
**Action:** Send `/restart-progress` to clear test data  
**Result:** Fresh start with clean database

### 2. Start Over with New Words
**Scenario:** You want to replace the word list  
**Steps:**
1. Send `/restart-progress` to clear progress
2. Update CSV file with new words
3. Restart app to import new words
4. Begin learning fresh words

### 3. Restart Learning Journey
**Scenario:** You want to review all words again  
**Action:** Send `/restart-progress`  
**Result:** All words become "new" again

---

## Testing

### Manual Testing

1. **Learn some words** (wait for scheduled jobs)
2. **Check database:**
   ```sql
   SELECT COUNT(*) FROM "LearnedWords";
   ```
3. **Send command:**
   ```
   /restart-progress
   ```
4. **Verify response** from bot
5. **Check database again:**
   ```sql
   SELECT COUNT(*) FROM "LearnedWords";
   -- Should be 0
   ```

### Test in Docker

```bash
# Start the bot
docker-compose up -d

# Check logs
docker-compose logs -f app

# In Telegram, send:
/restart-progress

# Watch logs for:
# "Processing /restart-progress command"
# "Progress restart completed. Removed X words"
```

---

## Logging

### Successful Execution
```
[INFO] Received message: /restart-progress
[INFO] Processing /restart-progress command
[INFO] Restarting learning progress - clearing all learned words
[INFO] Progress restarted. Removed 25 learned words
[INFO] Progress restart completed. Removed 25 words
```

### Error Case
```
[ERROR] Error handling restart-progress command
[ERROR] Exception details...
```

---

## Configuration

No additional configuration needed! The feature uses existing settings:

- `Telegram:ChatId` - Validates command origin
- `Language:SourceLanguageCode` - Determines response language

---

## Files Modified

1. **Created:**
   - `Services/ProgressService.cs`

2. **Updated:**
   - `Services/TelegramBotService.cs`
   - `Program.cs`
   - `README.md`
   - `QUICKSTART.md`

3. **Documentation:**
   - Updated with command usage examples
   - Added use cases and testing instructions

---

---

## Feature Added: Schedule Configuration Commands

### Overview
Each chat can now configure its own schedule for when repetition and new word messages are sent using `/set-repetition-time` and `/set-new-words-time` commands.

---

## What Was Implemented

### 1. Database Schema Update
**File:** `Data/Entities/ChatRegistration.cs`

Added schedule fields to ChatRegistration:
```csharp
public class ChatRegistration
{
    // ...existing fields...
    public string RepetitionTime { get; set; } = "09:00";
    public string NewWordsTime { get; set; } = "20:00";
}
```

### 2. New Bot Commands
**File:** `Services/TelegramBotService.cs`

**Added commands:**
- `/set-repetition-time HH:MM` - Set time for repetition messages
- `/set-new-words-time HH:MM` - Set time for new words messages

**Features:**
- Input validation (HH:MM format, valid hours 0-23, minutes 0-59)
- Localized responses (Ukrainian/English)
- Error handling with usage examples
- Per-chat configuration stored in database

### 3. ChatRegistrationService Extensions
**File:** `Services/ChatRegistrationService.cs`

New methods:
```csharp
Task UpdateRepetitionTimeAsync(long chatId, string time);
Task UpdateNewWordsTimeAsync(long chatId, string time);
Task<ChatRegistration?> GetChatRegistrationAsync(long chatId);
```

### 4. Job Scheduler Updates
**Files:** `Jobs/RepetitionJob.cs`, `Jobs/NewWordsJob.cs`

- Jobs now run **every minute** (Quartz cron: `"0 * * * * ?"`)
- Each job checks all registered chats
- Compares current time with each chat's configured time
- Only processes messages for chats with matching times
- Logs how many chats were processed

---

## How It Works

### Setting Repetition Time

1. **User sends command:**
   ```
   /set-repetition-time 09:30
   ```

2. **Bot validates:**
   - Checks format is HH:MM
   - Validates hour (0-23) and minute (0-59)
   - Updates database for this chat

3. **Bot responds (Ukrainian):**
   ```
   ✅ Час повторення встановлено на *09:30*
   
   Повторення буде надсилатися щодня о цій годині.
   ```

4. **Bot responds (English):**
   ```
   ✅ Repetition time set to *09:30*
   
   Repetitions will be sent daily at this time.
   ```

### Setting New Words Time

1. **User sends command:**
   ```
   /set-new-words-time 20:00
   ```

2. **Bot processes same as above**

3. **Bot responds (Ukrainian):**
   ```
   ✅ Час нових слів встановлено на *20:00*
   
   Нові слова будуть надсилатися щодня о цій годині.
   ```

---

## Code Examples

### Valid Commands
```
/set-repetition-time 09:00
/set-repetition-time 14:30
/set-repetition-time 23:59

/set-new-words-time 08:00
/set-new-words-time 20:30
/set-new-words-time 00:00
```

### Invalid Commands (with error messages)

**Missing time:**
```
/set-repetition-time
```
Response:
```
❌ Невірний формат команди.

Використання: `/set-repetition-time HH:MM`
Приклад: `/set-repetition-time 09:30`
```

**Invalid format:**
```
/set-repetition-time 9:00
/set-repetition-time 25:00
/set-repetition-time abc
```
Response:
```
❌ Невірний формат часу. Використовуйте HH:MM (наприклад, 09:30)
```

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

### Check Schedule Times
```sql
SELECT "ChatId", "ChatTitle", "RepetitionTime", "NewWordsTime" 
FROM "ChatRegistrations" 
WHERE "IsActive" = true;
```

Example output:
```
 ChatId      | ChatTitle        | RepetitionTime | NewWordsTime
-------------+------------------+----------------+--------------
 -1001234567 | My Learning Chat | 09:30          | 20:00
 -1007654321 | Another Group    | 08:00          | 19:00
```

---

## Job Execution Flow

### Every Minute
1. **RepetitionJob runs** (triggered by Quartz every minute)
2. Gets all active chat IDs
3. For each chat:
   - Reads chat's `RepetitionTime` from database
   - Compares with current time (HH:mm format)
   - If match → sends repetition message
   - If no match → skips this chat
4. Logs: "Processed X chat(s)"

### Example Log Output
```
[12:00:00] Starting repetition job check for all registered chats
[12:00:00] Current time: 12:00. Checking 3 chat(s) for scheduled repetition
[12:00:01] Starting repetition for chat -1001234567
[12:00:02] Repetition completed for chat -1001234567. Words repeated: 10
[12:00:02] Repetition job check completed. Processed 1 chat(s)
```

---

## Multi-Chat Support

Each chat maintains its own schedule independently:

| Chat ID      | Repetition Time | New Words Time | Timezone Notes          |
|--------------|-----------------|----------------|-------------------------|
| -1001234567  | 09:00          | 20:00          | Default times           |
| -1007654321  | 07:30          | 21:30          | Custom early/late       |
| -1009876543  | 12:00          | 18:00          | Lunch & evening         |

**Key Benefits:**
- Different groups can have different schedules
- No need to restart application
- Configuration via simple bot commands
- Changes take effect immediately (next minute check)

---

## Updated Help Command

```
/help
```

**Response (Ukrainian):**
```
📚 *Бот для вивчення мов*

🔹 Доступні команди:
• `/start-learning` - Розпочати навчання в цій групі
• `/stop-learning` - Зупинити навчання
• `/restart-progress` - Скинути прогрес навчання
• `/set-repetition-time HH:MM` - Встановити час повторення (наприклад, `/set-repetition-time 09:30`)
• `/set-new-words-time HH:MM` - Встановити час нових слів (наприклад, `/set-new-words-time 20:00`)

📝 Після реєстрації бот автоматично надсилає:
• Повторення вивчених слів
• Нові слова для вивчення

⏰ Ви можете налаштувати графік відправки за допомогою команд вище.
```

---

## Migration

### Migration File
`Migrations/20260130040313_AddScheduleTimesToChatRegistration.cs`

Adds two columns with default values:
```csharp
migrationBuilder.AddColumn<string>(
    name: "RepetitionTime",
    table: "ChatRegistrations",
    type: "text",
    nullable: false,
    defaultValue: "09:00");

migrationBuilder.AddColumn<string>(
    name: "NewWordsTime",
    table: "ChatRegistrations",
    type: "text",
    nullable: false,
    defaultValue: "20:00");
```

**Backward compatibility:** Existing chats automatically get default times (09:00 and 20:00).

---

## API Reference

### IChatRegistrationService (Extended)

```csharp
public interface IChatRegistrationService
{
    // ...existing methods...
    
    /// <summary>
    /// Updates repetition time for a chat
    /// </summary>
    Task UpdateRepetitionTimeAsync(long chatId, string time);
    
    /// <summary>
    /// Updates new words time for a chat
    /// </summary>
    Task UpdateNewWordsTimeAsync(long chatId, string time);
    
    /// <summary>
    /// Gets chat registration with schedule settings
    /// </summary>
    Task<ChatRegistration?> GetChatRegistrationAsync(long chatId);
}
```

---

## Future Enhancements

Possible extensions:

1. **Timezone support** - Store and respect each chat's timezone
2. **Multiple times per day** - Allow setting multiple schedule slots
3. **Day-specific schedules** - Different times for weekdays/weekends
4. **View current schedule** - `/show-schedule` command
5. **Pause schedule** - Temporarily disable without changing times
6. **Custom frequency** - Every N hours instead of daily

---

## Summary

✅ **Per-chat schedule configuration**  
✅ **Simple bot commands**  
✅ **Input validation**  
✅ **Localized for Ukrainian and English**  
✅ **Database persistence**  
✅ **Immediate effect (next minute)**  
✅ **Multi-chat support**  
✅ **Backward compatible**

Each chat can now configure its own learning schedule!

---

*Feature implemented: 2026-01-30*  
*Status: ✅ Ready for production*

---

## API Reference

### IProgressService

```csharp
public interface IProgressService
{
    /// <summary>
    /// Clears all learned words from the database
    /// </summary>
    /// <returns>Number of removed words</returns>
    Task<int> RestartProgressAsync();
}
```

### ITelegramBotService

```csharp
public interface ITelegramBotService
{
    Task SendMessageAsync(string text, ParseMode parseMode = ParseMode.Markdown);
    Task HandleUpdateAsync(Update update);
}
```

---

## Future Enhancements

Possible extensions:

1. **Confirmation dialog** before deleting progress
2. **Progress export** before restart
3. **Partial reset** (e.g., last N words only)
4. **Statistics** command to show learning progress
5. **Backup/restore** progress functionality

---

## Summary

✅ **Feature complete and tested**  
✅ **Localized for Ukrainian and English**  
✅ **Proper error handling**  
✅ **Comprehensive logging**  
✅ **Documented in guides**

Users can now easily reset their learning progress with a single command!

---

*Feature implemented: 2026-01-29*  
*Status: ✅ Ready for production*
