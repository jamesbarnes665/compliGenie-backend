using System;

namespace CompliGenie.Models
{
    public class Tenant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Subdomain { get; set; } = string.Empty;
        public string ApiKeyHash { get; set; } = string.Empty;
        public string? StripeAccountId { get; set; }
        public string Settings { get; set; } = "{}";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
