# Webhook Setup Guide

This guide explains how to configure Telegram webhooks for the bot.

## 📡 What is a Webhook?

A webhook is an HTTPS endpoint where Telegram sends updates (messages) to your bot. Instead of your bot constantly asking Telegram for new messages (polling), Telegram pushes them to your server.

**Benefits:**
- ✅ Instant message delivery
- ✅ Lower resource usage
- ✅ Better for production

**Requirements:**
- 🔒 HTTPS URL (HTTP not allowed by Telegram)
- 🌐 Publicly accessible domain

## 🚀 Production Setup

### Step 1: Get a Public Domain

You need a domain with HTTPS. Options:
- Your own domain with SSL certificate
- Cloud provider (AWS, GCP, Azure)
- PaaS (Heroku, Railway, Render)

### Step 2: Configure Webhook URL

In `docker-compose.yml` or environment variables:

```yaml
Telegram__WebhookUrl: "https://yourdomain.com/api/telegram/webhook"
```

**Important:** The URL must:
- Start with `https://` (not `http://`)
- End with `/api/telegram/webhook` (the bot's endpoint)
- Be publicly accessible from the internet

### Step 3: Deploy & Verify

1. Deploy your bot
2. Check logs for: `✅ Webhook set to: https://yourdomain.com/api/telegram/webhook`
3. Send a message to your bot - should respond instantly

## 🛠️ Local Development with ngrok

For testing locally, use **ngrok** to create a secure tunnel:

### Step 1: Install ngrok

**Windows:**
```bash
# Using Chocolatey
choco install ngrok

# Or download from https://ngrok.com
```

**macOS:**
```bash
brew install ngrok/ngrok/ngrok
```

**Linux:**
```bash
# Download and install
curl -s https://ngrok-agent.s3.amazonaws.com/ngrok.asc | sudo tee /etc/apt/trusted.gpg.d/ngrok.asc >/dev/null
echo "deb https://ngrok-agent.s3.amazonaws.com buster main" | sudo tee /etc/apt/sources.list.d/ngrok.list
sudo apt update && sudo apt install ngrok
```

### Step 2: Start ngrok

```bash
ngrok http 8888
```

You'll see output like:
```
Forwarding  https://abc123.ngrok.io -> http://localhost:8888
```

### Step 3: Configure Webhook

Copy the **HTTPS URL** and update `docker-compose.yml`:

```yaml
Telegram__WebhookUrl: "https://abc123.ngrok.io/api/telegram/webhook"
```

### Step 4: Restart Bot

```bash
docker-compose restart app
```

### Step 5: Test

Send a message to your bot - it should respond!

**⚠️ Important:** ngrok URLs change on each restart. You'll need to update the webhook URL and restart the bot whenever you restart ngrok.

### ngrok Free vs Paid

**Free tier:**
- ✅ Random subdomain (e.g., `abc123.ngrok.io`)
- ❌ URL changes on restart
- ✅ Good for testing

**Paid tier:**
- ✅ Fixed subdomain (e.g., `mybot.ngrok.io`)
- ✅ Custom domains
- ✅ Better for regular development

## 🔍 Troubleshooting

### Webhook Not Working

Check logs for errors:
```bash
docker-compose logs -f app
```

Common issues:

**1. URL not HTTPS**
```
❌ Error: Invalid webhook URL (must be HTTPS)
```
**Fix:** Ensure URL starts with `https://`

**2. URL not accessible**
```
❌ Error: Failed to set webhook
```
**Fix:** Verify the URL is publicly accessible. Test with:
```bash
curl https://yourdomain.com/api/telegram/webhook
```

**3. Wrong endpoint**
```
❌ 404 Not Found
```
**Fix:** Ensure URL ends with `/api/telegram/webhook`

### Verify Webhook Status

Use Telegram Bot API:
```bash
curl https://api.telegram.org/bot<YOUR_BOT_TOKEN>/getWebhookInfo
```

Response should show:
```json
{
  "ok": true,
  "result": {
    "url": "https://yourdomain.com/api/telegram/webhook",
    "has_custom_certificate": false,
    "pending_update_count": 0
  }
}
```

### Remove Webhook

To disable webhook:

**Option 1:** Set empty URL in config:
```yaml
Telegram__WebhookUrl: ""
```

**Option 2:** Use Telegram API:
```bash
curl https://api.telegram.org/bot<YOUR_BOT_TOKEN>/deleteWebhook
```

## 🔐 Security

### HTTPS Only

Telegram **requires** HTTPS for webhooks. Using HTTP will fail:
```
Error: Invalid webhook URL (must be HTTPS)
```

### Webhook Secret (Optional)

For extra security, you can add a secret token:

```yaml
Telegram__WebhookSecret: "your-secret-token"
```

Telegram will send this in the `X-Telegram-Bot-Api-Secret-Token` header.

## 📊 Webhook vs Polling

| Feature | Webhook | Polling |
|---------|---------|---------|
| **Speed** | Instant | Delayed (polling interval) |
| **Resources** | Low | Higher (constant requests) |
| **Setup** | Requires HTTPS domain | No setup |
| **Cost** | May need hosting | Free |
| **Development** | Needs ngrok | Easy |

**Recommendation:**
- 🧪 **Development:** Polling (simpler)
- 🚀 **Production:** Webhook (better performance)

## 🌐 Deployment Examples

### Railway.app
```yaml
Telegram__WebhookUrl: "https://your-app.railway.app/api/telegram/webhook"
```

### Heroku
```yaml
Telegram__WebhookUrl: "https://your-app.herokuapp.com/api/telegram/webhook"
```

### Render.com
```yaml
Telegram__WebhookUrl: "https://your-app.onrender.com/api/telegram/webhook"
```

### Custom Domain
```yaml
Telegram__WebhookUrl: "https://bot.yourdomain.com/api/telegram/webhook"
```

## ✅ Best Practices

1. **Always use HTTPS** - Telegram requirement
2. **Keep webhook URL secret** - Don't expose publicly
3. **Use environment variables** - Never hardcode tokens
4. **Monitor logs** - Check for webhook errors
5. **Test locally with ngrok** - Before production deployment
6. **Handle errors gracefully** - Bot should not crash on bad updates

## 📚 Additional Resources

- [Telegram Bot API - Webhooks](https://core.telegram.org/bots/api#setwebhook)
- [ngrok Documentation](https://ngrok.com/docs)
- [ASP.NET Core Hosting](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/)
