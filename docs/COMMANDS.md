# Bot Commands Feature - Implementation Summary

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
