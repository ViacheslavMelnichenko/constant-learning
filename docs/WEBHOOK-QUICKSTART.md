# Швидкий старт з Webhook

## Що маємо

✅ **TelegramController** - готовий webhook endpoint: `/api/telegram/webhook`  
✅ **WebhookConfigurationService** - автоматично реєструє webhook при старті  
✅ Все налаштовано, потрібен лише публічний URL  

---

## 3 кроки до запуску

### 1️⃣ Отримайте публічний URL

**Для локальної розробки (ngrok):**

```bash
# Встановіть ngrok: https://ngrok.com/download
ngrok http 8080
```

Скопіюйте URL, наприклад: `https://abc123.ngrok.io`

**Для production:**

Використовуйте свій домен з SSL: `https://yourdomain.com`

---

### 2️⃣ Налаштуйте WebhookUrl

**Варіант A - через docker-compose.yml:**

```yaml
environment:
  Telegram__BotToken: "ВАШ_ТОКЕН"
  Telegram__WebhookUrl: "https://abc123.ngrok.io/api/telegram/webhook"
```

**Варіант B - через змінну оточення:**

```bash
export TELEGRAM_WEBHOOK_URL="https://abc123.ngrok.io/api/telegram/webhook"
docker-compose up -d --build
```

**Варіант C - через user secrets (локально):**

```bash
cd ConstantLearning
dotnet user-secrets set "Telegram:WebhookUrl" "https://abc123.ngrok.io/api/telegram/webhook"
```

---

### 3️⃣ Запустіть і перевірте

```bash
# Запустіть
docker-compose up -d --build

# Перевірте логи
docker-compose logs -f app
```

Ви повинні побачити:
```
✅ Webhook successfully set to: https://abc123.ngrok.io/api/telegram/webhook
```

**Тестуйте в Telegram:**
```
/help
/start-learning
```

---

## Перевірка

### Чи webhook зареєстровано?

Відкрийте в браузері (замініть токен):
```
https://api.telegram.org/botВАШ_ТОКЕН/getWebhookInfo
```

Повинно бути:
```json
{
  "ok": true,
  "result": {
    "url": "https://abc123.ngrok.io/api/telegram/webhook",
    "pending_update_count": 0
  }
}
```

### Чи працює endpoint?

```bash
curl -I https://abc123.ngrok.io/api/telegram/webhook
```

Має повернути `405 Method Not Allowed` (це OK - endpoint приймає тільки POST)

---

## Troubleshooting

### "Webhook is not configured"

- Перевірте: `Telegram:WebhookUrl` встановлено в конфігурації
- Перегляньте логи: `docker-compose logs app | grep -i webhook`

### "Failed to set webhook"

- URL має бути HTTPS (ngrok автоматично дає HTTPS)
- URL має бути доступний з інтернету
- Перевірте: URL правильний

### "Повідомлення не доходять"

1. Перевірте webhook info (див. вище)
2. Перевірте логи: `docker-compose logs -f app`
3. Надішліть `/help` в Telegram
4. Шукайте в логах: `Received message`

---

## Важливо ⚠️

**Для ngrok:**
- URL змінюється при кожному перезапуску ngrok
- Потрібно оновити `WebhookUrl` і перезапустити додаток
- Для постійного URL - використовуйте платний ngrok або реальний домен

**Для production:**
- Використовуйте реальний домен
- Налаштуйте SSL (Let's Encrypt)
- Не змінюйте URL без потреби

---

## Альтернатива: Видалити webhook

Якщо хочете вимкнути webhook:

```bash
# Варіант 1: Через API
curl "https://api.telegram.org/botВАШ_ТОКЕН/deleteWebhook"

# Варіант 2: Залиште WebhookUrl порожнім
Telegram__WebhookUrl: ""
```

Тоді webhook не буде реєструватися автоматично.

---

📚 **Повна документація:** [WEBHOOK-SETUP.md](./WEBHOOK-SETUP.md)
