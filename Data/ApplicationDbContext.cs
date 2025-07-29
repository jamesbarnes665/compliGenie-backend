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
        public DbSet<Payment> Payments { get; set; }
        public DbSet<WebhookEvent> WebhookEvents { get; set; }
        public DbSet<FinancialAlert> FinancialAlerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Tenant
            modelBuilder.Entity<Tenant>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ApiKeyHash).IsUnique();
                entity.HasIndex(e => e.Subdomain).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Subdomain).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ApiKeyHash).IsRequired().HasMaxLength(255);
            });

            // Configure Policy with composite key for multi-tenancy
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.HasKey(e => new { e.TenantId, e.Id });
                entity.Property(e => e.ClientName).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Content).HasColumnType("TEXT");
            });

            // Configure Payment with composite key
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => new { e.TenantId, e.Id });
                entity.Property(e => e.Amount).HasPrecision(18, 2);
                entity.HasIndex(e => e.StripePaymentIntentId);
            });

            // Configure WebhookEvent with composite key
            modelBuilder.Entity<WebhookEvent>(entity =>
            {
                entity.HasKey(e => new { e.TenantId, e.Id });
                entity.HasIndex(e => e.StripeEventId).IsUnique();
                entity.Property(e => e.Payload).HasColumnType("TEXT");
            });

            // Configure FinancialAlert with composite key
            modelBuilder.Entity<FinancialAlert>(entity =>
            {
                entity.HasKey(e => new { e.TenantId, e.Id });
                entity.Property(e => e.Type).HasMaxLength(100);
                entity.Property(e => e.Severity).HasMaxLength(20);
            });
        }
    }
    
    // Thread-safe tenant context
    public static class CurrentTenant
    {
        private static readonly AsyncLocal<Guid?> _id = new AsyncLocal<Guid?>();
        
        public static Guid? Id
        {
            get => _id.Value;
            set => _id.Value = value;
        }
    }
}
