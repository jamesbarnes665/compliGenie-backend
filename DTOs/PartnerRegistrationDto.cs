using System.ComponentModel.DataAnnotations;

namespace CompliGenie.DTOs
{
    public class PartnerRegistrationDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string CompanyName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Url]
        public string Website { get; set; } = string.Empty;
        
        [Phone]
        public string? Phone { get; set; }
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        [Required]
        public string Industry { get; set; } = string.Empty;
        
        [Range(1, 10000)]
        public int? EstimatedMonthlyPolicies { get; set; }
    }
}
