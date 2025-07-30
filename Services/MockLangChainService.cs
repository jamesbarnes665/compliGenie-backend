using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CompliGenie.Models;
using System.Text.Json;

namespace CompliGenie.Services
{
    public class MockLangChainService : ILangChainService
    {
        private readonly ILogger<MockLangChainService> _logger;
        private readonly Random _random = new Random();

        public MockLangChainService(ILogger<MockLangChainService> logger)
        {
            _logger = logger;
        }

        public async Task<LLMResponse> Generate(GenerationRequest request)
        {
            _logger.LogInformation("Using MOCK LangChain service for testing");
            
            // Simulate processing delay
            await Task.Delay(_random.Next(500, 2000));

            // Generate mock policy based on request
            var policy = GenerateMockPolicy(request);
            
            return new LLMResponse
            {
                Content = JsonSerializer.Serialize(policy),
                TokensUsed = _random.Next(5000, 8000),
                Duration = TimeSpan.FromSeconds(_random.Next(2, 5))
            };
        }

        private object GenerateMockPolicy(GenerationRequest request)
        {
            // Extract industry from prompt if possible
            var industry = "general";
            if (request.Prompt.Contains("legal", StringComparison.OrdinalIgnoreCase))
                industry = "legal";
            else if (request.Prompt.Contains("healthcare", StringComparison.OrdinalIgnoreCase))
                industry = "healthcare";
            else if (request.Prompt.Contains("hr", StringComparison.OrdinalIgnoreCase))
                industry = "hr";

            return new
            {
                Title = $"AI Governance Policy - {industry.ToUpper()} Industry",
                Sections = GenerateSections(industry)
            };
        }

        private object[] GenerateSections(string industry)
        {
            var sections = new[]
            {
                new
                {
                    Title = "1. Executive Summary",
                    Content = $"This AI Governance Policy establishes comprehensive guidelines for the ethical and compliant use of artificial intelligence technologies within our {industry} organization. " +
                              "This policy ensures alignment with industry regulations, protects sensitive data, and maintains the highest standards of professional conduct. " +
                              "Our commitment to responsible AI usage reflects our dedication to innovation while prioritizing security, privacy, and ethical considerations. " +
                              "This policy applies to all employees, contractors, and partners who interact with AI systems in any capacity."
                },
                new
                {
                    Title = "2. AI Usage Guidelines", 
                    Content = $"All AI tools must be used in accordance with {industry}-specific regulations and best practices. " +
                              "Approved AI tools include: ChatGPT, Claude, and specialized {industry} AI applications. " +
                              "Employees must never input confidential or proprietary information into public AI systems. " +
                              "All AI-generated content must be reviewed and verified by qualified professionals before use. " +
                              "AI should augment human decision-making, not replace it entirely. " +
                              "Regular training on AI best practices is mandatory for all staff. " +
                              "Usage logs must be maintained for audit purposes."
                },
                new
                {
                    Title = "3. Data Protection & Privacy",
                    Content = "Data protection is paramount when using AI systems. All personal and sensitive data must be anonymized or pseudonymized before AI processing. " +
                              "Implement end-to-end encryption for data in transit and at rest. Regular security assessments must be conducted on all AI systems. " +
                              "Data retention policies must comply with applicable regulations. Access controls must be implemented based on the principle of least privilege. " +
                              "Incident response procedures must be established and tested regularly. Third-party AI vendors must sign data processing agreements."
                },
                new
                {
                    Title = "4. Compliance & Regulatory Requirements",
                    Content = $"Compliance with {industry}-specific regulations is mandatory. Regular compliance audits must be conducted quarterly. " +
                              "All AI systems must maintain detailed audit trails. Regulatory changes must be monitored and implemented promptly. " +
                              "Documentation must be maintained for all AI decision-making processes. External compliance assessments should be conducted annually. " +
                              "Non-compliance must be reported immediately to the compliance officer. Training on regulatory requirements is required for all relevant staff."
                },
                new
                {
                    Title = "5. Risk Management",
                    Content = "Comprehensive risk assessments must be conducted before deploying any AI system. Risk mitigation strategies must be documented and implemented. " +
                              "Regular monitoring of AI system performance is required. Bias detection and correction procedures must be in place. " +
                              "Business continuity plans must account for AI system failures. Insurance coverage should be reviewed to include AI-related risks. " +
                              "Vendor risk assessments are required for all third-party AI tools. Incident response teams must be trained on AI-specific scenarios."
                },
                new
                {
                    Title = "6. Training & Accountability",
                    Content = "All employees must complete AI ethics and usage training within 30 days of employment. Annual refresher training is mandatory. " +
                              "Department heads are accountable for their team's compliance. Clear escalation procedures must be established. " +
                              "Performance reviews should include AI usage compliance. Violations of this policy may result in disciplinary action. " +
                              "Recognition programs should reward responsible AI usage. Continuous improvement of training programs based on feedback and incidents."
                },
                new
                {
                    Title = "7. Monitoring & Reporting",
                    Content = "Continuous monitoring of AI system usage is required. Monthly reports on AI usage and compliance must be submitted to leadership. " +
                              "Key performance indicators for AI governance must be established and tracked. Regular stakeholder communications about AI initiatives and compliance status. " +
                              "Transparent reporting of any AI-related incidents or breaches. Benchmarking against industry best practices. " +
                              "Feedback mechanisms for employees to report concerns. Annual review and update of monitoring procedures."
                },
                new
                {
                    Title = "8. Implementation Timeline",
                    Content = "Phase 1 (Months 1-2): Policy adoption and initial training rollout. Phase 2 (Months 3-4): Tool implementation and access controls. " +
                              "Phase 3 (Months 5-6): Full compliance monitoring and reporting. Phase 4 (Ongoing): Continuous improvement and updates. " +
                              "Milestones will be tracked and reported to leadership monthly. Success metrics include training completion rates and compliance scores."
                }
            };

            // Add more sections to reach 8-12 pages
            if (industry == "legal")
            {
                var additionalSections = new[]
                {
                    new
                    {
                        Title = "9. Client Confidentiality",
                        Content = "Attorney-client privilege must be preserved in all AI interactions. Client data must never be used for AI training purposes. " +
                                  "Secure environments must be used for any AI processing of client information. Written client consent may be required for certain AI uses."
                    },
                    new
                    {
                        Title = "10. Ethical Considerations",
                        Content = "AI must not be used to provide legal advice without attorney supervision. Transparency about AI use must be maintained with clients. " +
                                  "AI bias in legal contexts must be actively monitored and mitigated. Professional responsibility rules apply to AI-assisted work."
                    }
                };
                var sectionsList = sections.ToList();
                sectionsList.AddRange(additionalSections);
                sections = sectionsList.ToArray();
            }

            return sections;
        }
    }
}
