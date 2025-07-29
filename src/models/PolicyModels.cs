using System;
using System.Collections.Generic;

namespace CompliGenie.Models
{
    public class PolicyRequest
    {
        public string ClientName { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public int CompanySize { get; set; }
        public string[] AITools { get; set; } = Array.Empty<string>();
        public string[] Jurisdictions { get; set; } = Array.Empty<string>();
        public string[] ComplianceFrameworks { get; set; } = Array.Empty<string>();
    }

    public class PolicyDocument
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Industry { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0";
        public List<PolicySection> Sections { get; set; } = new();
        public DateTime GeneratedAt { get; set; }
        public int PageCount { get; set; }
    }

    public class PolicySection
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Order { get; set; }
    }

    public class GenerationRequest
    {
        public string Prompt { get; set; } = string.Empty;
        public int MaxTokens { get; set; }
        public double Temperature { get; set; }
        public string ResponseFormat { get; set; } = string.Empty;
    }

    public class LLMResponse
    {
        public string Content { get; set; } = string.Empty;
        public int TokensUsed { get; set; }
        public TimeSpan Duration { get; set; }
    }
}
