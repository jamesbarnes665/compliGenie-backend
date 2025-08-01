using System;

namespace CompliGenie.Models
{
    public class TenantBranding
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string LogoUrl { get; set; } = string.Empty;
        public string LogoBase64 { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = "#000000";
        public string SecondaryColor { get; set; } = "#666666";
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPhone { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanyWebsite { get; set; } = string.Empty;
        public string FooterText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}