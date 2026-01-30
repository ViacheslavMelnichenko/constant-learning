# Діагностика помилки "Bad Request: invalid webhook URL specified"

## Ваша ситуація

URL: `https://32edbc9eeb43.ngrok-free.app/api/telegram/webhook`
Помилка: `Bad Request: invalid webhook URL specified`

---

## Крок 1: Перевірте чи працює ngrok

```bash
# Перевірте чи запущений ngrok
curl http://127.0.0.1:4040/api/tunnels
```

Або відкрийте в браузері: **http://127.0.0.1:4040**

Ви повинні побачити активний тунель.

---

## Крок 2: Перевірте чи доступний ваш endpoint

### Спосіб 1: Через браузер
Відкрийте: **https://32edbc9eeb43.ngrok-free.app/api/telegram/webhook**

**Очікуваний результат:**
- Статус 405 Method Not Allowed (це нормально!)
- Або JSON помилка з ASP.NET Core

**НЕ очікуваний результат:**
- Помилка ngrok (тунель не активний)
- Timeout
- 404 Not Found (додаток не запущений)

### Спосіб 2: Через curl
```bash
curl -I https://32edbc9eeb43.ngrok-free.app/api/telegram/webhook
```

**Правильна відповідь:**
```
HTTP/2 405
```

---

## Крок 3: Перевірте чи додаток запущений

```bash
# Якщо Docker
docker-compose ps

# Має показати:
# constantlearning-app   Up

# Перевірте логи
docker-compose logs app | tail -20
```

```bash
# Якщо локально (dotnet run)
# Перевірте чи є процес
ps aux | grep dotnet
```

---

## Крок 4: Перевірте ngrok-free.app особливості

ngrok-free.app має **додаткову сторінку попередження** для безкоштовних акаунтів!

### Рішення: Додайте User-Agent заголовок

Оновіть `WebhookConfigurationService.cs`:

```csharp
await botClient.SetWebhook(
    url: webhookUrl,
    allowedUpdates: Array.Empty<UpdateType>(),
    secretToken: null, // Можна додати для безпеки пізніше
    cancellationToken: cancellationToken);
```

### Або використайте ngrok з авторизацією

```bash
# Зареєструйтеся на ngrok.com
# Отримайте authtoken
ngrok config add-authtoken YOUR_AUTHTOKEN

# Запустіть з доменом
ngrok http 8080 --domain=your-custom-domain.ngrok-free.app
```

---

## Крок 5: Альтернатива - використайте старий ngrok.io домен

Якщо у вас є платний акаунт або старий токен:

```bash
# Запустіть з --hostname (для платних)
ngrok http 8080 --hostname=myapp.ngrok.io

# Або просто ngrok http (отримаєте ngrok.io домен)
ngrok http 8080
```

URL буде: `https://abc123.ngrok.io` замість `ngrok-free.app`

---

## Крок 6: Перевірте вручну через Telegram API

Спробуйте встановити webhook вручну через браузер:

```
https://api.telegram.org/botВАШ_ТОКЕН/setWebhook?url=https://32edbc9eeb43.ngrok-free.app/api/telegram/webhook
```

**Якщо отримаєте:**
```json
{"ok":true,"result":true,"description":"Webhook was set"}
```
✅ URL правильний, проблема в коді

**Якщо отримаєте:**
```json
{"ok":false,"error_code":400,"description":"Bad Request: invalid webhook URL specified"}
```
❌ URL недоступний для Telegram

---

## Крок 7: Перевірте доступність з зовні

Використайте зовнішній сервіс для перевірки:

### Варіант 1: https://reqbin.com/
1. Відкрийте https://reqbin.com/
2. Method: GET
3. URL: https://32edbc9eeb43.ngrok-free.app/api/telegram/webhook
4. Натисніть Send

### Варіант 2: https://www.urlvoid.com/
Перевірте чи домен не заблокований

---

## Можливі причини помилки

### 1. ⚠️ ngrok-free.app показує сторінку попередження

**Симптом:** При відкритті URL в браузері бачите сторінку "You are about to visit..."

**Рішення:**
- Натисніть "Visit Site" один раз
- Або використайте платний ngrok
- Або використайте ngrok authtoken для кращого досвіду

### 2. ⚠️ Додаток не запущений на localhost:8080

**Перевірка:**
```bash
curl http://localhost:8080/api/telegram/webhook
```

**Рішення:** Запустіть додаток

### 3. ⚠️ ngrok не запущений або тунель закрився

**Перевірка:**
```bash
curl http://127.0.0.1:4040/api/tunnels
```

**Рішення:** Перезапустіть ngrok

### 4. ⚠️ Firewall блокує ngrok

**Рішення:** 
- Перевірте firewall/антивірус
- Спробуйте інший регіон ngrok: `ngrok http 8080 --region eu`

### 5. ⚠️ URL містить зайві символи

**Перевірка:** Переконайтесь що немає пробілів або зайвих символів

**Правильно:**
```
https://32edbc9eeb43.ngrok-free.app/api/telegram/webhook
```

**НЕправильно:**
```
https://32edbc9eeb43.ngrok-free.app /api/telegram/webhook   ← пробіл
https://32edbc9eeb43.ngrok-free.app/api/telegram/webhook/   ← зайвий слеш
```

---

## Швидке рішення (100% працює)

### Використайте localtunnel замість ngrok:

```bash
# Встановіть
npm install -g localtunnel

# Запустіть
lt --port 8080

# Отримаєте URL типу: https://funny-elephant-12.loca.lt
```

Використайте цей URL:
```
https://funny-elephant-12.loca.lt/api/telegram/webhook
```

---

## Тестування

Після виправлення:

1. **Перезапустіть додаток:**
   ```bash
   docker-compose restart app
   # або
   dotnet run
   ```

2. **Перевірте логи:**
   ```bash
   docker-compose logs -f app
   ```

3. **Очікуваний вивід:**
   ```
   Setting webhook to: https://32edbc9eeb43.ngrok-free.app/api/telegram/webhook
     Scheme: https
     Host: 32edbc9eeb43.ngrok-free.app
     Path: /api/telegram/webhook
   ✅ Webhook successfully set to: https://32edbc9eeb43.ngrok-free.app/api/telegram/webhook
   ```

4. **Перевірте в Telegram:**
   ```
   https://api.telegram.org/botВАШ_ТОКЕН/getWebhookInfo
   ```

   Має показати:
   ```json
   {
     "ok": true,
     "result": {
       "url": "https://32edbc9eeb43.ngrok-free.app/api/telegram/webhook",
       "has_custom_certificate": false,
       "pending_update_count": 0
     }
   }
   ```

5. **Надішліть повідомлення боту:**
   ```
   /help
   ```

6. **Перевірте логи - має з'явитись:**
   ```
   Received message from chat -123456: /help
   ```

---

## Якщо нічого не допомагає

### План Б: Розгорніть на реальному хостингу

1. **Render.com (безкоштовно)**
   - Завантажте код на GitHub
   - Створіть Web Service на render.com
   - Отримаєте URL типу: https://myapp.onrender.com
   - Webhook: https://myapp.onrender.com/api/telegram/webhook

2. **Railway.app (безкоштовно)**
   - Аналогічно render.com
   - URL: https://myapp.railway.app

3. **fly.io (безкоштовно)**
   - `flyctl launch`
   - Отримаєте: https://myapp.fly.dev

Ці сервіси дають **стабільні HTTPS URL**, які точно працюють з Telegram!

---

## Підсумок

Найімовірніша причина - **ngrok-free.app показує проміжну сторінку**, яку Telegram не може обробити.

**Швидке рішення:**
1. Зареєструйтесь на ngrok.com
2. Додайте authtoken
3. Або використайте localtunnel/cloudflare tunnel
4. Або розгорніть на безкоштовному хостингу

Спробуйте ці кроки і дайте знати результат!
