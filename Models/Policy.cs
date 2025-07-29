using System;

namespace CompliGenie.Models
{
    public class Policy
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Content { get; set; } = "{}";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
