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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
