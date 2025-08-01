using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CompliGenie.Models;
using CompliGenie.Context;

namespace CompliGenie.Services
{
    public interface IPolicyGenerator
    {
        Task<PolicyDocument> GeneratePolicy(PolicyRequest request);
    }

    public class PolicyGenerator : IPolicyGenerator
    {
        private readonly ILangChainService _langChainService;
        private readonly ILogger<PolicyGenerator> _logger;
        private readonly ICurrentTenant _currentTenant;
        private readonly Dictionary<string, string> _industryPrompts = new()
        {
            ["legal"] = LoadPrompt("legal_8page_comprehensive.txt"),
            ["healthcare"] = LoadPrompt("healthcare_hipaa_focused.txt"),
            ["hr"] = LoadPrompt("hr_workplace_ai.txt"),
            ["insurance"] = LoadPrompt("insurance_risk_mitigation.txt"),
            ["general"] = LoadPrompt("general_business.txt")
        };

        public PolicyGenerator(
            ILangChainService langChainService,
            ILogger<PolicyGenerator> logger,
            ICurrentTenant currentTenant)
        {
            _langChainService = langChainService;
            _logger = logger;
            _currentTenant = currentTenant;
        }

        public async Task<PolicyDocument> GeneratePolicy(PolicyRequest request)
        {
            var basePrompt = _industryPrompts[request.Industry.ToLower()];
            var enrichedPrompt = EnrichPromptWithContext(basePrompt, request);
            
            var llmResponse = await _langChainService.Generate(new GenerationRequest
            {
                Prompt = enrichedPrompt,
                MaxTokens = 8000,
                Temperature = 0.7,
                ResponseFormat = "structured_json"
            });
            
            return ParseAndValidateResponse(llmResponse);
        }

        private static string LoadPrompt(string filename)
        {
            var path = Path.Combine("prompts", filename);
            if (!File.Exists(path))
            {
                // Return a default prompt if file not found
                return "Generate a comprehensive AI governance policy with proper sections.";
            }
            return File.ReadAllText(path);
        }

        private string EnrichPromptWithContext(string basePrompt, PolicyRequest request)
        {
            return basePrompt
                .Replace("{AI_TOOLS_LIST}", string.Join(", ", request.AITools ?? new[] { "AI Tools" }))
                .Replace("{COMPANY_SIZE}", request.CompanySize.ToString())
                .Replace("{JURISDICTIONS}", string.Join(", ", request.Jurisdictions ?? new[] { "US" }))
                .Replace("{COMPANY_NAME}", request.ClientName)
                .Replace("{TENANT_NAME}", _currentTenant.Name ?? "CompliGenie Partner");
        }

        private PolicyDocument ParseAndValidateResponse(LLMResponse response)
        {
            if (string.IsNullOrWhiteSpace(response.Content))
            {
                throw new InvalidOperationException("LLM returned empty response");
            }

            PolicyDocument doc;
            try
            {
                doc = JsonSerializer.Deserialize<PolicyDocument>(response.Content) ?? new PolicyDocument();
            }
            catch (JsonException)
            {
                doc = CreateDocumentFromText(response.Content);
            }

            doc.Id = Guid.NewGuid();
            doc.TenantId = _currentTenant.Id;
            doc.GeneratedAt = DateTime.UtcNow;
            doc.Version = "1.0";
            
            // Ensure sections exist
            if (doc.Sections == null || !doc.Sections.Any())
            {
                doc.Sections = new List<PolicySection>
                {
                    new PolicySection { Title = "AI Governance Policy", Content = response.Content, Order = 1 }
                };
            }
            
            var wordCount = doc.Sections.Sum(s => s.Content.Split(' ').Length);
            doc.PageCount = Math.Max(8, (int)Math.Ceiling(wordCount / 250.0));

            return doc;
        }

        private PolicyDocument CreateDocumentFromText(string text)
        {
            var sections = new List<PolicySection>();
            var lines = text.Split('\n');
            
            PolicySection? currentSection = null;
            var contentBuilder = new StringBuilder();
            
            foreach (var line in lines)
            {
                if (IsHeaderLine(line))
                {
                    if (currentSection != null)
                    {
                        currentSection.Content = contentBuilder.ToString().Trim();
                        sections.Add(currentSection);
                        contentBuilder.Clear();
                    }
                    currentSection = new PolicySection
                    {
                        Title = line.Trim(),
                        Order = sections.Count + 1
                    };
                }
                else if (currentSection != null)
                {
                    contentBuilder.AppendLine(line);
                }
            }
            
            if (currentSection != null)
            {
                currentSection.Content = contentBuilder.ToString().Trim();
                sections.Add(currentSection);
            }

            return new PolicyDocument
            {
                Title = "AI Governance Policy",
                Sections = sections
            };
        }

        private bool IsHeaderLine(string? line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;
                
            return line.StartsWith("#") || 
                   System.Text.RegularExpressions.Regex.IsMatch(line, @"^\d+\.");
        }
    }
}