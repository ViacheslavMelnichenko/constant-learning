using ConstantLearning.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ConstantLearning.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Word> Words => Set<Word>();
    public DbSet<LearnedWord> LearnedWords => Set<LearnedWord>();
    public DbSet<BotConfiguration> BotConfigurations => Set<BotConfiguration>();
    public DbSet<ChatRegistration> ChatRegistrations => Set<ChatRegistration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Word>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FrequencyRank);
            entity.Property(e => e.TargetWord).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SourceMeaning).HasMaxLength(500).IsRequired();
            entity.Property(e => e.PhoneticTranscription).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<LearnedWord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ChatId, e.WordId }).IsUnique();
            entity.HasOne(e => e.Word)
                .WithMany()
                .HasForeignKey(e => e.WordId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BotConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Value).HasMaxLength(500).IsRequired();
        });

        modelBuilder.Entity<ChatRegistration>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ChatId).IsUnique();
            entity.Property(e => e.ChatTitle).HasMaxLength(200);
        });
    }
}