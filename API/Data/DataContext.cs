using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class DataContext: DbContext
{
    public DataContext(DbContextOptions options): base(options)
    {
        
    }

    public DbSet<AppUser?> Users { get; set; }
    public DbSet<UserLike?> Likes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserLike>().HasKey(like => new { like.SourceUserId, like.TargetUserId });
        modelBuilder.Entity<UserLike>()
            .HasOne(like => like.SourceUser)
            .WithMany(user => user.LikedUsers)
            .HasForeignKey(like => like.SourceUserId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<UserLike>()
            .HasOne(like => like.TargetUser)
            .WithMany(user => user.LikedByUsers)
            .HasForeignKey(like => like.TargetUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}