using Microsoft.EntityFrameworkCore;
using VirtualShot.Entities;

namespace VirtualShot.Database;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomMember> RoomMembers => Set<RoomMember>();
    public DbSet<ShotEntry> Shots => Set<ShotEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.Property(u => u.Nickname).HasMaxLength(32);
            entity.Property(u => u.Emoji).HasMaxLength(8);
            entity.HasIndex(u => u.Nickname);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.Property(r => r.Code).HasMaxLength(6);
            entity.HasIndex(r => r.Code).IsUnique();
            entity.Property(r => r.Name).HasMaxLength(64);
            entity.HasOne(r => r.Host)
                .WithMany()
                .HasForeignKey(r => r.HostId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RoomMember>(entity =>
        {
            entity.HasIndex(m => new { m.RoomId, m.UserId }).IsUnique();
            entity.HasOne(m => m.Room)
                .WithMany(r => r.Members)
                .HasForeignKey(m => m.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ShotEntry>(entity =>
        {
            entity.Property(s => s.Drink).HasMaxLength(16);
            entity.HasIndex(s => new { s.RoomId, s.TakenAt });
            entity.HasOne(s => s.Room)
                .WithMany(r => r.Shots)
                .HasForeignKey(s => s.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
