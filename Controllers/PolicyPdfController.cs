using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CompliGenie.Services;
using CompliGenie.Context;
using CompliGenie.Models;
using CompliGenie.Data;
using System.Collections.Generic;

namespace CompliGenie.Controllers
{
    [ApiController]
    [Route("api/policies")]
    public class PolicyPdfController : ControllerBase
    {
        private readonly IPdfGenerationService _pdfService;
        private readonly ILogger<PolicyPdfController> _logger;
        private readonly ICurrentTenant _currentTenant;

        public PolicyPdfController(
            IPdfGenerationService pdfService,
            ILogger<PolicyPdfController> logger,
            ICurrentTenant currentTenant)
        {
            _pdfService = pdfService;
            _logger = logger;
            _currentTenant = currentTenant;
        }

        [HttpGet("{policyId}/pdf")]
        public async Task<IActionResult> GeneratePolicyPdf(Guid policyId)
        {
            try
            {
                if (!_currentTenant.IsSet)
                {
                    return Unauthorized("No tenant context");
                }

                // Create a comprehensive test policy document
                var policy = new PolicyDocument
                {
                    Id = policyId,
                    TenantId = _currentTenant.Id,
                    ClientName = "Acme Legal Associates LLP",
                    Title = "AI Governance & Compliance Policy",
                    Version = "1.0",
                    GeneratedAt = DateTime.UtcNow,
                    PageCount = 12,
                    Sections = new List<PolicySection>
                    {
                        new PolicySection
                        {
                            Order = 1,
                            Title = "Executive Summary",
                            Content = @"<p>This AI Governance Policy establishes comprehensive guidelines and procedures for the responsible development, deployment, and use of artificial intelligence technologies within Acme Legal Associates LLP. This policy ensures compliance with applicable laws, ethical standards, and industry best practices while maximizing the benefits of AI technology for our clients and stakeholders.</p>
                            
                            <h3>Key Objectives</h3>
                            <ul>
                                <li>Establish clear governance structures for AI initiatives</li>
                                <li>Ensure ethical and responsible AI use across all departments</li>
                                <li>Maintain client confidentiality and data privacy</li>
                                <li>Comply with all relevant regulations and professional standards</li>
                                <li>Promote transparency and accountability in AI decision-making</li>
                            </ul>
                            
                            <h3>Scope</h3>
                            <p>This policy applies to all employees, contractors, and third-party vendors who develop, deploy, or use AI systems on behalf of Acme Legal Associates LLP. It covers all AI technologies including but not limited to machine learning models, natural language processing systems, predictive analytics, and automated decision-making tools.</p>
                            
                            <h3>Policy Statement</h3>
                            <p>Acme Legal Associates LLP is committed to leveraging AI technology to enhance our legal services while maintaining the highest standards of professional ethics, client confidentiality, and regulatory compliance. We recognize that AI systems must be developed and used in ways that are transparent, accountable, and aligned with our core values of integrity, excellence, and client service.</p>"
                        },
                        new PolicySection
                        {
                            Order = 2,
                            Title = "AI Usage Guidelines",
                            Content = @"<h3>2.1 Approved AI Tools and Applications</h3>
                            <p>The following AI tools have been approved for use within our organization after thorough security and compliance review:</p>
                            
                            <h4>2.1.1 Document Analysis and Review</h4>
                            <ul>
                                <li><strong>Contract Analysis AI:</strong> For reviewing and analyzing legal contracts, identifying key terms, and flagging potential issues</li>
                                <li><strong>Due Diligence Assistant:</strong> For accelerating document review in M&A transactions and corporate matters</li>
                                <li><strong>Legal Research Tools:</strong> Including Westlaw Edge, Lexis+, and CaseText for AI-enhanced legal research</li>
                            </ul>
                            
                            <h4>2.1.2 Legal Writing and Drafting</h4>
                            <ul>
                                <li><strong>Brief Assistant:</strong> For initial draft generation of legal briefs and memoranda</li>
                                <li><strong>Contract Drafting Tools:</strong> For creating first drafts of standard agreements</li>
                                <li><strong>Citation Checker:</strong> For verifying legal citations and formatting</li>
                            </ul>
                            
                            <h4>2.1.3 Client Communication</h4>
                            <ul>
                                <li><strong>Client Intake Chatbot:</strong> For initial client inquiries and appointment scheduling</li>
                                <li><strong>Email Assistant:</strong> For drafting routine client communications (with mandatory human review)</li>
                            </ul>
                            
                            <h3>2.2 Prohibited Uses</h3>
                            <p>The following uses of AI are strictly prohibited:</p>
                            <ul>
                                <li>Making final legal decisions without human attorney review</li>
                                <li>Providing legal advice directly to clients without attorney supervision</li>
                                <li>Processing sensitive client data through non-approved AI systems</li>
                                <li>Using AI to generate court filings without thorough human review and verification</li>
                                <li>Sharing confidential client information with AI systems that are not covered by appropriate confidentiality agreements</li>
                            </ul>
                            
                            <h3>2.3 Human Oversight Requirements</h3>
                            <p>All AI-generated content must be reviewed by a qualified attorney before:</p>
                            <ul>
                                <li>Submission to any court or regulatory body</li>
                                <li>Delivery to clients as legal advice</li>
                                <li>Inclusion in any binding legal document</li>
                                <li>Publication or external distribution</li>
                            </ul>"
                        },
                        new PolicySection
                        {
                            Order = 3,
                            Title = "Data Protection and Privacy",
                            Content = @"<h3>3.1 Client Data Handling</h3>
                            <p>When using AI systems with client data, all personnel must adhere to the following requirements:</p>
                            
                            <h4>3.1.1 Data Classification</h4>
                            <p>All client data must be classified according to sensitivity level:</p>
                            <ul>
                                <li><strong>Highly Confidential:</strong> Attorney-client privileged communications, litigation strategy, sensitive personal information</li>
                                <li><strong>Confidential:</strong> General client matters, internal memoranda, financial records</li>
                                <li><strong>Internal Use:</strong> Administrative data, scheduling information, general correspondence</li>
                            </ul>
                            
                            <h4>3.1.2 AI System Requirements</h4>
                            <p>AI systems processing client data must:</p>
                            <ul>
                                <li>Be hosted in secure, SOC 2 Type II certified environments</li>
                                <li>Employ end-to-end encryption for data in transit and at rest</li>
                                <li>Maintain detailed audit logs of all data access and processing</li>
                                <li>Comply with applicable data residency requirements</li>
                                <li>Be covered by appropriate Business Associate Agreements (BAAs) where applicable</li>
                            </ul>
                            
                            <h3>3.2 Data Retention and Deletion</h3>
                            <p>AI systems must implement the following data retention policies:</p>
                            <ul>
                                <li>Training data containing client information must be deleted after model training is complete</li>
                                <li>AI-generated outputs must be retained according to standard document retention policies</li>
                                <li>Audit logs must be retained for a minimum of 7 years</li>
                                <li>Client data must be permanently deleted from AI systems upon client request or matter closure</li>
                            </ul>
                            
                            <h3>3.3 Cross-Border Data Transfers</h3>
                            <p>When AI systems involve cross-border data processing:</p>
                            <ul>
                                <li>Ensure compliance with GDPR requirements for EU client data</li>
                                <li>Implement Standard Contractual Clauses (SCCs) where required</li>
                                <li>Obtain explicit client consent for international data transfers</li>
                                <li>Maintain records of all cross-border data flows</li>
                            </ul>"
                        },
                        new PolicySection
                        {
                            Order = 4,
                            Title = "Compliance and Regulatory Requirements",
                            Content = @"<h3>4.1 Legal and Professional Standards</h3>
                            <p>All AI use must comply with:</p>
                            
                            <h4>4.1.1 Bar Association Rules</h4>
                            <ul>
                                <li><strong>Model Rule 1.1 (Competence):</strong> Attorneys must understand AI capabilities and limitations</li>
                                <li><strong>Model Rule 1.6 (Confidentiality):</strong> Client information must be protected when using AI</li>
                                <li><strong>Model Rule 5.1 (Supervisory Responsibilities):</strong> Partners must ensure proper AI oversight</li>
                                <li><strong>Model Rule 5.3 (Responsibilities Regarding Nonlawyer Assistance):</strong> AI systems are treated as nonlawyer assistants</li>
                            </ul>
                            
                            <h4>4.1.2 Jurisdiction-Specific Requirements</h4>
                            <p>Additional compliance requirements by jurisdiction:</p>
                            <ul>
                                <li><strong>California:</strong> Compliance with CCPA and SB 1001 (bot disclosure law)</li>
                                <li><strong>New York:</strong> Adherence to SHIELD Act requirements for data security</li>
                                <li><strong>European Union:</strong> GDPR compliance for all EU client matters</li>
                                <li><strong>Illinois:</strong> Biometric Information Privacy Act (BIPA) compliance</li>
                            </ul>
                            
                            <h3>4.2 Industry-Specific Regulations</h3>
                            <p>When serving clients in regulated industries, ensure AI compliance with:</p>
                            <ul>
                                <li><strong>Healthcare:</strong> HIPAA requirements for protected health information</li>
                                <li><strong>Financial Services:</strong> SOX, Dodd-Frank, and SEC regulations</li>
                                <li><strong>Government Contracts:</strong> FAR and DFARS requirements</li>
                            </ul>
                            
                            <h3>4.3 Audit and Compliance Monitoring</h3>
                            <p>The firm will conduct:</p>
                            <ul>
                                <li>Quarterly reviews of AI system compliance</li>
                                <li>Annual third-party security assessments</li>
                                <li>Regular testing of data protection measures</li>
                                <li>Ongoing monitoring of regulatory changes affecting AI use</li>
                            </ul>"
                        },
                        new PolicySection
                        {
                            Order = 5,
                            Title = "Training and Accountability",
                            Content = @"<h3>5.1 Mandatory Training Programs</h3>
                            <p>All personnel using AI systems must complete:</p>
                            
                            <h4>5.1.1 Initial Training Requirements</h4>
                            <ul>
                                <li><strong>AI Fundamentals for Legal Professionals</strong> (4 hours)
                                    <ul>
                                        <li>Understanding AI capabilities and limitations</li>
                                        <li>Ethical considerations in legal AI use</li>
                                        <li>Identifying appropriate use cases</li>
                                    </ul>
                                </li>
                                <li><strong>Data Security and Privacy in AI</strong> (2 hours)
                                    <ul>
                                        <li>Protecting client confidentiality</li>
                                        <li>Secure AI system usage</li>
                                        <li>Incident reporting procedures</li>
                                    </ul>
                                </li>
                                <li><strong>Tool-Specific Training</strong> (varies by tool)
                                    <ul>
                                        <li>Hands-on training for each approved AI tool</li>
                                        <li>Best practices and common pitfalls</li>
                                        <li>Quality control procedures</li>
                                    </ul>
                                </li>
                            </ul>
                            
                            <h4>5.1.2 Ongoing Education</h4>
                            <ul>
                                <li>Annual refresher training (2 hours)</li>
                                <li>Updates on new AI tools and features</li>
                                <li>Lessons learned from AI incidents</li>
                                <li>Regulatory and ethical developments</li>
                            </ul>
                            
                            <h3>5.2 Roles and Responsibilities</h3>
                            
                            <h4>5.2.1 AI Governance Committee</h4>
                            <p>Responsible for:</p>
                            <ul>
                                <li>Approving new AI tools and use cases</li>
                                <li>Reviewing compliance reports</li>
                                <li>Updating policies and procedures</li>
                                <li>Investigating incidents and violations</li>
                            </ul>
                            
                            <h4>5.2.2 Department Heads</h4>
                            <p>Responsible for:</p>
                            <ul>
                                <li>Ensuring team compliance with AI policies</li>
                                <li>Identifying training needs</li>
                                <li>Reporting AI-related issues</li>
                                <li>Implementing quality control measures</li>
                            </ul>
                            
                            <h4>5.2.3 Individual Users</h4>
                            <p>Responsible for:</p>
                            <ul>
                                <li>Completing required training</li>
                                <li>Following all usage guidelines</li>
                                <li>Reporting concerns or incidents</li>
                                <li>Maintaining professional judgment</li>
                            </ul>
                            
                            <h3>5.3 Violation Consequences</h3>
                            <p>Violations of this policy may result in:</p>
                            <ul>
                                <li>Mandatory retraining</li>
                                <li>Suspension of AI system access</li>
                                <li>Formal disciplinary action</li>
                                <li>Termination for serious or repeated violations</li>
                                <li>Reporting to relevant bar authorities if required</li>
                            </ul>"
                        },
                        new PolicySection
                        {
                            Order = 6,
                            Title = "Implementation Timeline and Next Steps",
                            Content = @"<h3>6.1 Implementation Phases</h3>
                            
                            <h4>Phase 1: Foundation (Months 1-2)</h4>
                            <ul>
                                <li>Establish AI Governance Committee</li>
                                <li>Complete initial policy training for all staff</li>
                                <li>Audit current AI tool usage</li>
                                <li>Implement basic monitoring systems</li>
                            </ul>
                            
                            <h4>Phase 2: Tool Deployment (Months 3-4)</h4>
                            <ul>
                                <li>Deploy approved AI tools to pilot groups</li>
                                <li>Gather feedback and refine processes</li>
                                <li>Develop tool-specific guidelines</li>
                                <li>Create quality control checklists</li>
                            </ul>
                            
                            <h4>Phase 3: Full Implementation (Months 5-6)</h4>
                            <ul>
                                <li>Roll out AI tools firm-wide</li>
                                <li>Implement comprehensive monitoring</li>
                                <li>Establish regular audit schedule</li>
                                <li>Launch ongoing training program</li>
                            </ul>
                            
                            <h3>6.2 Success Metrics</h3>
                            <p>We will measure success through:</p>
                            <ul>
                                <li>Training completion rates (target: 100% within 60 days)</li>
                                <li>AI tool adoption rates (target: 80% of eligible users)</li>
                                <li>Efficiency improvements (target: 30% reduction in routine task time)</li>
                                <li>Client satisfaction scores (maintain or improve current levels)</li>
                                <li>Compliance audit results (target: zero critical findings)</li>
                            </ul>
                            
                            <h3>6.3 Review and Updates</h3>
                            <p>This policy will be reviewed and updated:</p>
                            <ul>
                                <li>Quarterly by the AI Governance Committee</li>
                                <li>Annually by the Executive Committee</li>
                                <li>As needed based on regulatory changes</li>
                                <li>Following any significant AI-related incidents</li>
                            </ul>
                            
                            <p><strong>For questions about this policy or AI usage, contact:</strong><br/>
                            AI Governance Committee<br/>
                            Email: ai-governance@acmelegal.com<br/>
                            Phone: (555) 123-4567 ext. 890</p>"
                        }
                    }
                };

                // Get tenant branding (enhanced for testing)
                var branding = new TenantBranding
                {
                    TenantId = _currentTenant.Id,
                    CompanyName = _currentTenant.Name ?? "CompliGenie Partner",
                    PrimaryColor = "#0066CC",
                    SecondaryColor = "#666666",
                    LogoUrl = "https://via.placeholder.com/200x60/0066CC/FFFFFF?text=ACME+LEGAL",
                    CompanyAddress = "123 Legal Plaza, Suite 500\nNew York, NY 10001",
                    CompanyPhone = "(555) 123-4567",
                    CompanyEmail = "compliance@acmelegal.com",
                    CompanyWebsite = "www.acmelegal.com",
                    FooterText = "This document is confidential and proprietary. Unauthorized distribution is prohibited."
                };

                var pdfBytes = await _pdfService.GeneratePdf(policy, branding);

                return File(pdfBytes, "application/pdf", $"AI_Governance_Policy_{policy.ClientName.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for policy {PolicyId}", policyId);
                return StatusCode(500, new { error = "Failed to generate PDF" });
            }
        }
    }
}