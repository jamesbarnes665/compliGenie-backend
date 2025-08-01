using Microsoft.EntityFrameworkCore;
using CompliGenie.Models;

namespace CompliGenie.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Policy> Policies { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure Tenant entity
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Subdomain).IsUnique();
                entity.HasIndex(e => e.ApiKeyHash).IsUnique();
            });
            
            // Configure Policy entity
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.HasKey(e => new { e.TenantId, e.Id });
                entity.HasOne<Tenant>()
                    .WithMany()
                    .HasForeignKey(e => e.TenantId);
            });
        }
    }
}