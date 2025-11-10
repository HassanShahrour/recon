using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Reconova.Data.Models;

namespace Reconova.Data
{
    public class ReconovaDbContext : IdentityDbContext<User>
    {
        public ReconovaDbContext(DbContextOptions<ReconovaDbContext> options)
        : base(options) { }

        public DbSet<ScanResult> ScanResults { get; set; }
        public DbSet<AIResult> AIResults { get; set; }
        public DbSet<Tool> Tools { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Pc> Pcs { get; set; }
        public DbSet<Skill> Skill { get; set; }
        public DbSet<UserFollowing> UserFollowing { get; set; }
        public DbSet<Post> Post { get; set; }
        public DbSet<Like> Like { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<ChatMessage> ChatMessage { get; set; }
        public DbSet<Tasks> Tasks { get; set; }
        public DbSet<Notification> Notification { get; set; }
        public DbSet<Plan> Plan { get; set; }
        public DbSet<ScheduledScan> ScheduledScan { get; set; }
        public DbSet<ScheduledTool> ScheduledTool { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tasks → User (many-to-1)
            modelBuilder.Entity<Tasks>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ScanResult → Task (many-to-1)
            modelBuilder.Entity<ScanResult>()
                .HasOne(sr => sr.Task)
                .WithMany(t => t.ScanResults)
                .HasForeignKey(sr => sr.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // AIResult → Task (many-to-1)
            modelBuilder.Entity<AIResult>()
                .HasOne(ar => ar.Task)
                .WithMany(t => t.AIResults)
                .HasForeignKey(ar => ar.TaskId)
                .OnDelete(DeleteBehavior.Cascade);


            // User → ScanResults (1-to-many)
            modelBuilder.Entity<ScanResult>()
                .HasOne(sr => sr.User)
                .WithMany(u => u.ScanResults)
                .HasForeignKey(sr => sr.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User → AIResults (1-to-many)
            modelBuilder.Entity<AIResult>()
                .HasOne(ar => ar.User)
                .WithMany(u => u.AIResults)
                .HasForeignKey(ar => ar.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ScanResult → AIResult (1-to-1)
            modelBuilder.Entity<ScanResult>()
                .HasOne(sr => sr.AIResult)
                .WithOne(ar => ar.ScanResult)
                .HasForeignKey<AIResult>(ar => ar.ScanId)
                .HasPrincipalKey<ScanResult>(sr => sr.ScanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tool → Category (many-to-1)
            modelBuilder.Entity<Tool>()
                .HasOne(t => t.Category)
                .WithMany()
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserFollowing configuration
            modelBuilder.Entity<UserFollowing>()
                .HasOne(uf => uf.Follower)
                .WithMany(u => u.Following)
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserFollowing>()
                .HasOne(uf => uf.Followee)
                .WithMany(u => u.Followers)
                .HasForeignKey(uf => uf.FolloweeId)
                .OnDelete(DeleteBehavior.Restrict);


            // Message configuration
            modelBuilder.Entity<ChatMessage>()
               .HasOne(m => m.Sender)
               .WithMany(u => u.SentMessages)
               .HasForeignKey(m => m.SenderId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Recipient)
                .WithMany(u => u.ReceivedMessages)
                .HasForeignKey(m => m.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Skill>()
             .HasOne(s => s.User)
             .WithMany(u => u.Skills)
             .HasForeignKey(s => s.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Post>()
                .HasMany(p => p.Media)
                .WithOne(m => m.Post)
                .HasForeignKey(m => m.PostId);
        }


    }
}
