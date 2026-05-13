using AIAgent.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AIAgent.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<AccessLog> AccessLogs { get; set; }
    public DbSet<IpStatistics> IpStatistics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ChatMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
        });

        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasMany(e => e.Messages)
                .WithOne()
                .HasForeignKey(e => e.SessionId);
        });

        modelBuilder.Entity<AccessLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IpAddress);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
        });

        modelBuilder.Entity<IpStatistics>(entity =>
        {
            entity.HasKey(e => e.IpAddress);
            entity.HasIndex(e => e.LastAccess);
        });
    }
}
