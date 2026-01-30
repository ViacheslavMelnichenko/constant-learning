# Як працює ngrok і як налаштувати webhook

## Що таке ngrok?

**ngrok** - це інструмент, який створює **тунель** від публічного інтернету до вашого локального сервера.

```
Telegram → Internet → ngrok → ваш localhost:8080
```

### Чому потрібен ngrok для локальної розробки?

1. **Ваш бот на localhost** - Telegram не може достукатись до `localhost`
2. **ngrok створює публічний URL** - наприклад `https://abc123.ngrok.io`
3. **Весь трафік перенаправляється** на ваш локальний `localhost:8080`
4. **Автоматично HTTPS** - Telegram вимагає HTTPS для webhook

---

## Швидкий старт з ngrok

### Крок 1: Встановіть ngrok

**Windows:**
1. Завантажте: https://ngrok.com/download
2. Розпакуйте `ngrok.exe`
3. Додайте в PATH або запускайте з папки

**macOS (Homebrew):**
```bash
brew install ngrok/ngrok/ngrok
```

**Linux:**
```bash
curl -s https://ngrok-agent.s3.amazonaws.com/ngrok.asc | sudo tee /etc/apt/trusted.gpg.d/ngrok.asc >/dev/null
echo "deb https://ngrok-agent.s3.amazonaws.com buster main" | sudo tee /etc/apt/sources.list.d/ngrok.list
sudo apt update && sudo apt install ngrok
```

### Крок 2: Створіть безкоштовний акаунт (опціонально, але рекомендовано)

1. Зареєструйтесь: https://dashboard.ngrok.com/signup
2. Скопіюйте authtoken
3. Підключіть токен:
   ```bash
   ngrok config add-authtoken YOUR_AUTHTOKEN
   ```

### Крок 3: Запустіть ngrok

```bash
ngrok http 8080
```

**Ви побачите:**
```
Session Status                online
Account                       Your Name (Plan: Free)
Version                       3.x.x
Region                        Europe (eu)
Latency                       -
Web Interface                 http://127.0.0.1:4040
Forwarding                    https://abc123.ngrok.io -> http://localhost:8080
```

**Важливо:** 
- Ваш публічний URL: `https://abc123.ngrok.io`
- Webhook endpoint: `https://abc123.ngrok.io/api/telegram/webhook`

### Крок 4: Налаштуйте WebhookUrl

**Варіант A - через user secrets (рекомендовано для локальної розробки):**

```bash
cd ConstantLearning
dotnet user-secrets set "Telegram:WebhookUrl" "https://abc123.ngrok.io/api/telegram/webhook"
```

**Варіант B - через appsettings.Development.json:**

```json
{
  "Telegram": {
    "WebhookUrl": "https://abc123.ngrok.io/api/telegram/webhook"
  }
}
```

**Варіант C - через docker-compose.yml:**

```yaml
environment:
  Telegram__WebhookUrl: "https://abc123.ngrok.io/api/telegram/webhook"
```

### Крок 5: Запустіть додаток

```bash
# Якщо локально без Docker:
dotnet run

# Якщо з Docker:
docker-compose up -d --build
```

### Крок 6: Перевірте логи

```bash
# Локально
# Дивіться вивід в консолі

# Docker
docker-compose logs -f app
```

**Очікуваний вивід:**
```
[INF] ✅ Webhook successfully set to: https://abc123.ngrok.io/api/telegram/webhook
[INF] Webhook info: URL=https://abc123.ngrok.io/api/telegram/webhook, PendingUpdates=0
```

### Крок 7: Тестуйте!

Відкрийте ваш Telegram бот і надішліть:
```
/help
/start-learning
```

**Перевірте логи - ви повинні побачити:**
```
[INF] Received message from chat -1001234567: /help
```

---

## ngrok Web Interface

ngrok автоматично запускає веб-інтерфейс на `http://127.0.0.1:4040`

**Відкрийте в браузері:** http://127.0.0.1:4040

Ви побачите:
- Усі HTTP запити до вашого бота
- Тіло запитів від Telegram
- Відповіді вашого сервера
- Час виконання кожного запиту

**Це дуже корисно для дебагу!**

---

## Важливі моменти про ngrok

### ⚠️ URL змінюється при перезапуску

Кожен раз, коли ви **перезапускаєте ngrok**, ви отримуєте **новий URL**.

**Приклад:**
- Перший запуск: `https://abc123.ngrok.io`
- Другий запуск: `https://xyz789.ngrok.io` ← новий URL!

**Що робити:**
1. Оновіть `Telegram:WebhookUrl` з новим URL
2. Перезапустіть додаток (він автоматично оновить webhook)

### ✅ Як отримати фіксований URL (платна версія)

```bash
ngrok http 8080 --domain=your-app.ngrok.io
```

Платні плани: https://ngrok.com/pricing

### 🆓 Обмеження безкоштовної версії

- ✅ 1 одночасний тунель
- ✅ HTTPS
- ✅ 40 з'єднань/хвилину
- ❌ URL змінюється при перезапуску
- ❌ Без custom domain

Для локальної розробки - цього достатньо!

---

## Альтернативи ngrok

### 1. **localtunnel**
```bash
npm install -g localtunnel
lt --port 8080
```

### 2. **Cloudflare Tunnel**
```bash
cloudflared tunnel --url http://localhost:8080
```

### 3. **serveo.net**
```bash
ssh -R 80:localhost:8080 serveo.net
```

Але **ngrok найпопулярніший і найпростіший!**

---

## Повний workflow: Локальна розробка з ngrok

### Термінал 1: Запустіть ngrok
```bash
ngrok http 8080

# Скопіюйте URL, наприклад: https://abc123.ngrok.io
```

### Термінал 2: Налаштуйте та запустіть додаток
```bash
cd ConstantLearning

# Встановіть bot token (один раз)
dotnet user-secrets set "Telegram:BotToken" "ВАШ_ТОКЕН"

# Встановіть webhook URL (оновлюйте при перезапуску ngrok)
dotnet user-secrets set "Telegram:WebhookUrl" "https://abc123.ngrok.io/api/telegram/webhook"

# Запустіть
dotnet run
```

### Термінал 3 (опціонально): Моніторинг
```bash
# Дивіться логи в реальному часі
dotnet run | grep -i "webhook\|received\|message"
```

### Браузер: ngrok Web Interface
Відкрийте: http://127.0.0.1:4040

---

## Troubleshooting

### ❌ "tunnel session failed: your account is limited to 1 simultaneous ngrok agent"

**Проблема:** Ви вже запустили ngrok в іншому місці.

**Рішення:**
```bash
# Знайдіть процес
ps aux | grep ngrok

# Вбийте процес
kill <PID>

# Або перезапустіть ngrok
```

### ❌ "ERR_NGROK_3004"

**Проблема:** Порт вже використовується.

**Рішення:**
```bash
# Перевірте, що запущено на порту 8080
lsof -i :8080  # macOS/Linux
netstat -ano | findstr :8080  # Windows

# Змініть порт
ngrok http 8081
```

### ❌ Webhook не реєструється

**Перевірте:**
1. ngrok запущений
2. URL правильний (з `/api/telegram/webhook`)
3. Додаток запущений
4. Логи на наявність помилок

### ❌ "Failed to complete tunnel connection"

**Проблема:** Немає інтернет-з'єднання або блокується firewall.

**Рішення:**
- Перевірте інтернет
- Вимкніть VPN (якщо конфліктує)
- Перезапустіть ngrok

---

## Перевірка, що все працює

### 1. Перевірте ngrok запущений
```bash
curl http://127.0.0.1:4040/api/tunnels
```

### 2. Перевірте публічний URL доступний
```bash
curl -I https://abc123.ngrok.io/api/telegram/webhook
# Має повернути: 405 Method Not Allowed (це OK!)
```

### 3. Перевірте webhook зареєстровано
```
https://api.telegram.org/botВАШ_ТОКЕН/getWebhookInfo
```

Має показати:
```json
{
  "ok": true,
  "result": {
    "url": "https://abc123.ngrok.io/api/telegram/webhook",
    "pending_update_count": 0
  }
}
```

### 4. Надішліть тестове повідомлення боту
```
/help
```

### 5. Перевірте в ngrok Web Interface
http://127.0.0.1:4040 - ви повинні побачити POST запит від Telegram

---

## Коли НЕ потрібен ngrok

**Production:** Використовуйте реальний домен з SSL
**CI/CD:** Використовуйте тестові моки
**Polling:** Якщо використовуєте polling замість webhook

**ngrok потрібен тільки для локальної розробки з webhook!**

---

## Готово! 🎉

Тепер ви знаєте:
- ✅ Що таке ngrok
- ✅ Як встановити та запустити
- ✅ Як налаштувати webhook
- ✅ Як дебажити через Web Interface
- ✅ Як вирішувати проблеми

**Успішної розробки!** 🚀
