# Налаштування Telegram Webhook

## Що таке Webhook?

Webhook - це спосіб, коли Telegram **сам надсилає** повідомлення на ваш сервер (замість того, щоб ваш сервер постійно запитував Telegram).

**Ваш TelegramController вже готовий!** Endpoint: `https://your-domain.com/api/telegram/webhook`

---

## Варіанти налаштування

### Варіант 1: Автоматичне налаштування (Рекомендовано)

Додаток автоматично зареєструє webhook при старті, якщо вказано `Telegram:WebhookUrl`.

#### Крок 1: Отримати публічний URL

**Для локальної розробки - використовуйте ngrok:**

```bash
# Встановіть ngrok (якщо ще не встановлено)
# https://ngrok.com/download

# Запустіть ngrok для порту 8080
ngrok http 8080
```

Ви побачите:
```
Forwarding  https://abc123.ngrok.io -> http://localhost:8080
```

Ваш webhook URL буде: `https://abc123.ngrok.io/api/telegram/webhook`

**Для production - використовуйте свій домен:**
```
https://yourdomain.com/api/telegram/webhook
```

#### Крок 2: Налаштувати WebhookUrl

**Варіант A: Через docker-compose.yml**

```yaml
environment:
  Telegram__BotToken: "ВАШ_ТОКЕН"
  Telegram__WebhookUrl: "https://abc123.ngrok.io/api/telegram/webhook"
```

**Варіант B: Через змінну оточення**

```bash
export TELEGRAM_WEBHOOK_URL="https://abc123.ngrok.io/api/telegram/webhook"
```

**Варіант C: Через appsettings.json (НЕ рекомендовано для production)**

```json
{
  "Telegram": {
    "BotToken": "ВАШ_ТОКЕН",
    "WebhookUrl": "https://abc123.ngrok.io/api/telegram/webhook"
  }
}
```

**Варіант D: Через User Secrets (для локальної розробки)**

```bash
cd ConstantLearning
dotnet user-secrets set "Telegram:WebhookUrl" "https://abc123.ngrok.io/api/telegram/webhook"
```

#### Крок 3: Запустити додаток

```bash
docker-compose up -d --build
```

#### Крок 4: Перевірити логи

```bash
docker-compose logs -f app
```

Ви повинні побачити:
```
[INF] ✅ Webhook successfully set to: https://abc123.ngrok.io/api/telegram/webhook
[INF] Webhook info: URL=https://abc123.ngrok.io/api/telegram/webhook, PendingUpdates=0
```

#### Крок 5: Тестувати

Надішліть в Telegram групу:
```
/help
/start-learning
```

Перевірте логи:
```bash
docker-compose logs app | grep -i "received\|message"
```

---

### Варіант 2: Ручне налаштування webhook

Якщо не хочете використовувати автоматичне налаштування:

#### Спосіб A: Через браузер

Відкрийте в браузері (замініть `YOUR_BOT_TOKEN` та URL):
```
https://api.telegram.org/botYOUR_BOT_TOKEN/setWebhook?url=https://yourdomain.com/api/telegram/webhook
```

Відповідь:
```json
{
  "ok": true,
  "result": true,
  "description": "Webhook was set"
}
```

#### Спосіб B: Через curl

```bash
curl -X POST "https://api.telegram.org/botYOUR_BOT_TOKEN/setWebhook" \
  -H "Content-Type: application/json" \
  -d '{"url":"https://yourdomain.com/api/telegram/webhook"}'
```

#### Спосіб C: Через Postman/Insomnia

```
POST https://api.telegram.org/botYOUR_BOT_TOKEN/setWebhook
Content-Type: application/json

{
  "url": "https://yourdomain.com/api/telegram/webhook"
}
```

---

## Перевірка налаштувань

### Перевірити поточний webhook

**Через браузер:**
```
https://api.telegram.org/botYOUR_BOT_TOKEN/getWebhookInfo
```

**Відповідь (успішно налаштовано):**
```json
{
  "ok": true,
  "result": {
    "url": "https://yourdomain.com/api/telegram/webhook",
    "has_custom_certificate": false,
    "pending_update_count": 0,
    "max_connections": 40
  }
}
```

**Відповідь (НЕ налаштовано):**
```json
{
  "ok": true,
  "result": {
    "url": "",
    "has_custom_certificate": false,
    "pending_update_count": 0
  }
}
```

### Видалити webhook (якщо потрібно)

```
https://api.telegram.org/botYOUR_BOT_TOKEN/deleteWebhook
```

---

## Налаштування для різних середовищ

### Локальна розробка з ngrok

```bash
# Термінал 1: Запустіть додаток
docker-compose up

# Термінал 2: Запустіть ngrok
ngrok http 8080

# Термінал 3: Встановіть webhook
export TELEGRAM_WEBHOOK_URL="https://abc123.ngrok.io/api/telegram/webhook"
docker-compose restart app
```

**Важливо:** Кожен раз, коли перезапускаєте ngrok, URL змінюється! Потрібно оновити webhook.

### Production на сервері

**1. Налаштуйте reverse proxy (nginx/traefik)**

nginx приклад:
```nginx
server {
    listen 80;
    server_name yourdomain.com;
    
    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
    }
}
```

**2. Налаштуйте SSL (Let's Encrypt)**

```bash
certbot --nginx -d yourdomain.com
```

**3. Встановіть webhook**

```yaml
# docker-compose.yml
environment:
  Telegram__WebhookUrl: "https://yourdomain.com/api/telegram/webhook"
```

### Kubernetes

```yaml
# configmap.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: constantlearning-config
data:
  Telegram__WebhookUrl: "https://yourdomain.com/api/telegram/webhook"
```

---

## Troubleshooting

### ❌ Webhook не працює

**1. Перевірте URL доступний ззовні:**
```bash
curl -I https://yourdomain.com/api/telegram/webhook
```

Має повернути: `HTTP/1.1 405 Method Not Allowed` (це нормально - webhook приймає тільки POST)

**2. Перевірте логи додатку:**
```bash
docker-compose logs -f app
```

**3. Перевірте webhook info:**
```
https://api.telegram.org/botYOUR_BOT_TOKEN/getWebhookInfo
```

Якщо `last_error_message` присутнє - читайте помилку.

### ❌ "Webhook is already deleted"

Це нормально, якщо видаляєте неіснуючий webhook.

### ❌ "Bad Request: bad webhook: HTTPS url must be provided for webhook"

Telegram вимагає HTTPS! Використовуйте:
- ngrok (автоматично дає HTTPS)
- Reverse proxy з SSL
- Cloudflare Tunnel

### ❌ "Bad Request: bad webhook: Failed to resolve host"

URL недоступний для Telegram серверів. Перевірте:
- URL правильний
- Сервер доступний з інтернету
- Firewall не блокує Telegram IP

### ❌ "Conflict: can't use getUpdates method while webhook is active"

Webhook активний. Якщо хочете використовувати polling - видаліть webhook:
```
https://api.telegram.org/botYOUR_BOT_TOKEN/deleteWebhook
```

### ❌ Повідомлення не доходять

**1. Перевірте endpoint доступний:**
```bash
curl -X POST https://yourdomain.com/api/telegram/webhook \
  -H "Content-Type: application/json" \
  -d '{"update_id":1}'
```

**2. Перевірте логи на помилки:**
```bash
docker-compose logs app | grep -i error
```

**3. Надішліть тестове повідомлення боту**

**4. Перевірте pending updates:**
```
https://api.telegram.org/botYOUR_BOT_TOKEN/getWebhookInfo
```

Якщо `pending_update_count > 0` - є необроблені повідомлення.

---

## Порівняння: Webhook vs Polling

| Критерій | Webhook | Polling |
|----------|---------|---------|
| **Швидкість** | ✅ Миттєво | ❌ Затримка до 1 сек |
| **Навантаження на сервер** | ✅ Мінімальне | ❌ Постійні запити |
| **Публічний URL** | ❌ Потрібен | ✅ Не потрібен |
| **SSL/HTTPS** | ❌ Обов'язково | ✅ Не потрібен |
| **Локальна розробка** | ❌ Потрібен ngrok | ✅ Працює одразу |
| **Production** | ✅ Рекомендовано | ❌ Не рекомендовано |
| **Складність** | ⚠️ Середня | ✅ Проста |

---

## Швидкий старт

### Для локальної розробки з ngrok:

```bash
# 1. Запустіть ngrok
ngrok http 8080

# 2. Скопіюйте URL (наприклад: https://abc123.ngrok.io)

# 3. Додайте в docker-compose.yml
Telegram__WebhookUrl: "https://abc123.ngrok.io/api/telegram/webhook"

# 4. Перезапустіть
docker-compose up -d --build

# 5. Перевірте логи
docker-compose logs -f app

# 6. Тестуйте в Telegram
/help
```

### Для production:

```bash
# 1. Налаштуйте домен + SSL

# 2. Додайте в docker-compose.yml
Telegram__WebhookUrl: "https://yourdomain.com/api/telegram/webhook"

# 3. Розгорніть
docker-compose up -d --build

# 4. Перевірте
curl https://api.telegram.org/botYOUR_TOKEN/getWebhookInfo
```

---

## Приклад повного workflow

### Крок за кроком з ngrok:

```bash
# 1. Отримайте токен від @BotFather
# TOKEN: 1234567890:ABCdefGHIjklMNOpqrsTUVwxyz

# 2. Запустіть ngrok
ngrok http 8080
# URL: https://abc123.ngrok.io

# 3. Налаштуйте user secrets
cd ConstantLearning
dotnet user-secrets set "Telegram:BotToken" "1234567890:ABCdefGHIjklMNOpqrsTUVwxyz"
dotnet user-secrets set "Telegram:WebhookUrl" "https://abc123.ngrok.io/api/telegram/webhook"

# 4. Запустіть локально (без Docker для тестування)
dotnet run

# Або з Docker:
# Відредагуйте docker-compose.yml:
# Telegram__BotToken: "1234567890:ABCdefGHIjklMNOpqrsTUVwxyz"
# Telegram__WebhookUrl: "https://abc123.ngrok.io/api/telegram/webhook"

docker-compose up -d --build

# 5. Перевірте логи
docker-compose logs -f app

# Ви повинні побачити:
# ✅ Webhook successfully set to: https://abc123.ngrok.io/api/telegram/webhook

# 6. Перевірте webhook
curl "https://api.telegram.org/bot1234567890:ABCdefGHIjklMNOpqrsTUVwxyz/getWebhookInfo"

# 7. Тестуйте в Telegram
# Додайте бота в групу
# Надішліть: /help

# 8. Перевірте логи
docker-compose logs app | grep "Received message"
```

---

## Готово! 🎉

Тепер ваш бот працює через webhook і отримує повідомлення миттєво від Telegram!

**Важливо:** Якщо використовуєте ngrok, пам'ятайте:
- URL змінюється при кожному перезапуску ngrok
- Для production використовуйте реальний домен з SSL
