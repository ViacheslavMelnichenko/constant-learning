# Database Migration Guide

## Creating Initial Migration

Since the database schema uses generic column names (`TargetWord`, `SourceMeaning`), you need to create a fresh migration.

### Option 1: Using Docker Compose (Recommended)

Docker Compose will automatically run migrations on startup. No manual steps needed.

```bash
docker-compose up -d
```

The `DatabaseInitializationService` will:
1. Apply all migrations
2. Import words from CSV
3. Start the application

### Option 2: Manual Migration (Development)

If you're developing locally without Docker:

```powershell
# Navigate to project directory
cd C:\projects\constant-learning\ConstantLearning\ConstantLearning

# Remove old migrations (if any)
Remove-Item -Recurse -Force .\Migrations

# Create fresh migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

## Migration Details

The migration will create these tables:

### Words
```sql
CREATE TABLE "Words" (
    "Id" SERIAL PRIMARY KEY,
    "TargetWord" VARCHAR(200) NOT NULL,
    "SourceMeaning" VARCHAR(500) NOT NULL,
    "PhoneticTranscription" VARCHAR(200) NOT NULL,
    "FrequencyRank" INTEGER NOT NULL,
    "ImportedAt" TIMESTAMP NOT NULL
);

CREATE INDEX "IX_Words_FrequencyRank" ON "Words" ("FrequencyRank");
```

### LearnedWords
```sql
CREATE TABLE "LearnedWords" (
    "Id" SERIAL PRIMARY KEY,
    "WordId" INTEGER NOT NULL,
    "LearnedAt" TIMESTAMP NOT NULL,
    "LastRepeatedAt" TIMESTAMP NOT NULL,
    "RepetitionCount" INTEGER NOT NULL,
    FOREIGN KEY ("WordId") REFERENCES "Words" ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_LearnedWords_WordId" ON "LearnedWords" ("WordId");
```

### BotConfigurations
```sql
CREATE TABLE "BotConfigurations" (
    "Id" SERIAL PRIMARY KEY,
    "Key" VARCHAR(100) NOT NULL,
    "Value" VARCHAR(500) NOT NULL,
    "UpdatedAt" TIMESTAMP NOT NULL
);

CREATE UNIQUE INDEX "IX_BotConfigurations_Key" ON "BotConfigurations" ("Key");
```

## Verifying Migration

Connect to PostgreSQL:

```bash
# Using Docker
docker exec -it constantlearning-postgres psql -U postgres -d constantlearning

# List tables
\dt

# Check Words table structure
\d "Words"

# Check word count
SELECT COUNT(*) FROM "Words";

# Check learned words
SELECT COUNT(*) FROM "LearnedWords";

# Exit
\q
```

## Resetting Database

### With Docker Compose

```bash
# Stop and remove volumes
docker-compose down -v

# Start fresh
docker-compose up -d
```

### Manual Reset

```powershell
# Drop database
dotnet ef database drop --force

# Recreate
dotnet ef database update
```

## Troubleshooting

### Migration fails with "Table already exists"

```bash
docker-compose down -v
docker-compose up -d
```

### CSV import doesn't work

Check logs:
```bash
docker-compose logs app | Select-String -Pattern "import"
```

Verify CSV path in container:
```bash
docker exec -it constantlearning-app ls -la /app/data/
```

### Connection string issues

Verify environment variables:
```bash
docker exec -it constantlearning-app env | Select-String -Pattern "Connection"
```

## Production Migration Strategy

For production deployments:

1. **Backup existing database** (if updating from old schema)
2. **Create migration scripts** for data transformation
3. **Test in staging** environment first
4. **Apply migrations** before deploying new code
5. **Monitor logs** for migration errors

### Kubernetes Migration Job

Create a separate Job to run migrations:

```yaml
apiVersion: batch/v1
kind: Job
metadata:
  name: constantlearning-migration
spec:
  template:
    spec:
      containers:
      - name: migration
        image: constantlearning:latest
        command: ["dotnet", "ef", "database", "update"]
        env:
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: constantlearning-secret
              key: CONNECTION_STRING
      restartPolicy: OnFailure
```

Run before deploying the main application.
