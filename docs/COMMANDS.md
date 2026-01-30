# Bot Commands Reference

Complete reference for all Telegram bot commands.

## Getting Started

### `/start-learning`

Register your group to start receiving daily word lessons.

**Usage:**
```
/start-learning
```

**Response:**
```
✅ Чат успішно зареєстровано!

📚 Бот буде надсилати:
• Повторення вивчених слів о 09:00
• Нові слова для вивчення о 20:00

⏰ Налаштувати час відправки:
• /set-repetition-time HH:MM - час повторення
• /set-new-words-time HH:MM - час нових слів

Гарного навчання! 🎯
```

**Notes:**
- Group must be registered before receiving scheduled messages
- Each group has independent progress
- Default schedule: 09:00 (repetition), 20:00 (new words)

---

### `/stop-learning`

Pause scheduled messages for this group.

**Usage:**
```
/stop-learning
```

**Response:**
```
✅ Навчання зупинено

📭 Бот більше не надсилатиме повідомлення в цей чат.

Ваш прогрес збережено. Використайте /start-learning щоб продовжити навчання.
```

**Notes:**
- Progress is saved and not deleted
- Use `/start-learning` to resume

---

### `/restart-progress`

Clear all learning progress for this group and start from scratch.

**Usage:**
```
/restart-progress
```

**Response:**
```
✅ Прогрес скинуто!

Видалено 42 вивчених слів.
Починаємо навчання спочатку! 🎯
```

**Warning:** This deletes all learned words for this group!

---

## Schedule Configuration

### `/set-repetition-time`

Set the time when daily repetition messages are sent.

**Usage:**
```
/set-repetition-time HH:MM
```

**Examples:**
```
/set-repetition-time 09:00
/set-repetition-time 08:30
/set-repetition-time 12:00
```

**Response:**
```
✅ Час повторення встановлено на 09:00

Повторення буде надсилатися щодня о цій годині.
```

**Notes:**
- Time format: 24-hour (00:00 to 23:59)
- Each group can have different schedule
- Changes take effect immediately

---

### `/set-new-words-time`

Set the time when daily new words are sent.

**Usage:**
```
/set-new-words-time HH:MM
```

**Examples:**
```
/set-new-words-time 20:00
/set-new-words-time 19:30
/set-new-words-time 21:00
```

**Response:**
```
✅ Час нових слів встановлено на 20:00

Нові слова будуть надсилатися щодня о цій годині.
```

**Notes:**
- Time format: 24-hour (00:00 to 23:59)
- Independent from repetition time
- Recommended: evening time for better retention

---

## Help

### `/help`

Show all available commands.

**Usage:**
```
/help
```

---

## Command Summary

| Command | Description | Requires Registration |
|---------|-------------|:---------------------:|
| `/start-learning` | Register group | No |
| `/stop-learning` | Pause messages | Yes |
| `/restart-progress` | Clear progress | Yes |
| `/set-repetition-time HH:MM` | Set repetition time | Yes |
| `/set-new-words-time HH:MM` | Set new words time | Yes |
| `/help` | Show commands | No |

## Error Messages

### Chat Not Registered

```
ℹ️ Цей чат ще не зареєстровано для навчання.

Використайте /start-learning щоб розпочати.
```

**Solution:** Send `/start-learning` first

### Invalid Time Format

```
❌ Невірний формат часу. Використовуйте HH:MM (наприклад, 09:30)
```

**Solution:** Use 24-hour format with colon (e.g., `09:00`, `14:30`)

---

## Examples

### Complete Setup

```
# 1. Register group
/start-learning

# 2. Set custom schedule
/set-repetition-time 08:00
/set-new-words-time 19:00

# 3. Start learning!
# Bot will send messages automatically
```

### Reset and Restart

```
# Clear all progress
/restart-progress

# Optionally adjust schedule
/set-repetition-time 10:00

# Learning continues with new schedule
```

### Pause Learning

```
# Pause messages
/stop-learning

# Resume when ready
/start-learning
```

---

## Daily Messages

### Repetition Flow (Default: 09:00)

```
📚 Повторення — згадайте переклад:
1. бути
2. мати
3. робити
...

[30 seconds later]

✅ Відповіді:

1.  бути      →  być  [być]
2.  мати      →  mieć [mjeć]
3.  робити    →  robić [robić]
...
```

### New Words Flow (Default: 20:00)

```
🆕 Нові слова:

1.  tak   [tak]   → так
2.  dla   [dla]   → для
3.  więc  [vjenʦ] → отже
```

---

## Tips

✅ **Best Practices:**
- Set repetition time in the morning for better recall
- Set new words time in the evening before sleep
- Don't restart progress too often - consistency is key!
- Each group can have different schedules

❌ **Avoid:**
- Changing schedule too frequently
- Restarting progress without reason
- Setting same time for both flows (they'll overlap)

---

## Need Help?

- 📖 [Quick Start Guide](QUICKSTART.md)
- 🌐 [Webhook Setup](WEBHOOK-SETUP.md)
- 🏠 [Main README](../README.md)
- 💬 Open an issue on GitHub
