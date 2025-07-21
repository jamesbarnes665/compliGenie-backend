# backend/app/services/policy_generator.py

from typing import List, Dict, Any
import asyncio
from datetime import datetime

class PolicyGenerator:
    """
    Generates AI policy content based on company information.
    This is a placeholder that you'll enhance with LangChain/GPT integration.
    """
    
    async def generate(
        self, 
        company_name: str, 
        industry: str, 
        ai_tools: List[str], 
        employee_count: int
    ) -> Dict[str, Any]:
        """
        Generate policy content structure for PDF generation.
        
        Args:
            company_name: Name of the company
            industry: Industry type
            ai_tools: List of AI tools the company uses
            employee_count: Number of employees
            
        Returns:
            Dictionary containing policy structure with sections
        """
        
        # Format AI tools list
        tools_text = ", ".join(ai_tools) if ai_tools else "AI tools"
        
        # Generate policy content
        policy_data = {
            "title": f"Artificial Intelligence Usage Policy",
            "company_name": company_name,
            "effective_date": datetime.now().strftime("%B %d, %Y"),
            "sections": [
                {
                    "title": "Purpose and Scope",
                    "content": f"""This Artificial Intelligence (AI) Usage Policy establishes comprehensive guidelines and standards for the responsible use of AI technologies at {company_name}. As a {industry} company with {employee_count} employees, we recognize the transformative potential of AI while acknowledging the need for careful governance.

This policy applies to all employees, contractors, consultants, and third-party users who interact with AI systems on behalf of {company_name}. It covers the use of {tools_text} and any other AI technologies adopted by the organization.

The purpose of this policy is to ensure that AI technologies are used in a manner that aligns with our corporate values, complies with applicable laws and regulations, protects sensitive information, and maintains the trust of our stakeholders.""",
                    "subsections": []
                },
                {
                    "title": "Approved AI Tools and Technologies",
                    "content": f"""The following AI tools and technologies have been evaluated and approved for use within {company_name}: {tools_text}. Each tool has been assessed for security, privacy, and compliance with our organizational standards.

Employees must only use AI tools that have been officially approved through our technology review process. Any request to use new AI tools must be submitted to the IT department for evaluation before implementation.

The use of personal or unapproved AI tools for company business is strictly prohibited, as these may not meet our security and compliance requirements.""",
                    "subsections": [
                        {
                            "title": "Tool-Specific Guidelines",
                            "content": f"Each approved AI tool has specific use cases and limitations. Employees must familiarize themselves with the guidelines for each tool they use, including data input restrictions, output verification requirements, and appropriate use cases."
                        }
                    ]
                },
                {
                    "title": "Data Security and Privacy",
                    "content": f"""Protection of sensitive data is paramount when using AI technologies. Employees must never input the following types of information into AI systems:

• Personally Identifiable Information (PII) of customers, employees, or partners
• Confidential business information, trade secrets, or proprietary data
• Financial records, credit card numbers, or banking information
• Protected Health Information (PHI) or medical records
• Legal documents under attorney-client privilege
• Source code or technical specifications of proprietary systems

All AI-generated content must be treated as potentially public information. Assume that any data input into AI systems could be stored, analyzed, or used for training purposes by the AI provider.""",
                    "subsections": []
                },
                {
                    "title": "Acceptable Use Guidelines",
                    "content": f"""AI tools should be used to enhance productivity, improve decision-making, and support innovation while maintaining professional standards. Acceptable uses include:

• Drafting and editing business communications
• Conducting research and analysis
• Generating ideas and creative content
• Automating routine tasks
• Improving code quality and documentation
• Enhancing customer service responses

All AI-generated content must be reviewed, verified, and edited by qualified personnel before being used in any official capacity. Employees remain fully responsible for any content they create or distribute, regardless of AI assistance.""",
                    "subsections": []
                },
                {
                    "title": "Compliance and Regulatory Requirements",
                    "content": f"""As a {industry} company, {company_name} must comply with industry-specific regulations regarding AI use. This includes:

• Maintaining audit trails of AI-assisted decisions
• Ensuring transparency in AI-driven processes
• Protecting against algorithmic bias and discrimination
• Meeting data retention and deletion requirements
• Complying with international data protection regulations

Employees must understand and follow all regulatory requirements relevant to their role and use of AI technologies.""",
                    "subsections": []
                },
                {
                    "title": "Intellectual Property and Attribution",
                    "content": f"""AI-generated content may raise complex intellectual property questions. When using AI tools:

• Verify ownership rights of AI-generated content
• Properly attribute AI assistance when required
• Ensure compliance with licensing terms of AI tools
• Protect {company_name}'s intellectual property from unauthorized disclosure
• Respect third-party intellectual property rights

AI should not be used to plagiarize, infringe copyrights, or misappropriate others' work.""",
                    "subsections": []
                },
                {
                    "title": "Monitoring and Enforcement",
                    "content": f"""{company_name} reserves the right to monitor the use of AI tools to ensure compliance with this policy. Violations may result in:

• Revocation of AI tool access
• Mandatory retraining
• Disciplinary action up to and including termination
• Legal action for serious breaches

All employees are expected to report suspected violations of this policy to their supervisor or the compliance department.""",
                    "subsections": []
                },
                {
                    "title": "Training and Support",
                    "content": f"""All employees who use AI tools must complete mandatory training on:

• This AI Usage Policy
• Tool-specific guidelines and best practices
• Data security and privacy requirements
• Ethical considerations in AI use
• Industry-specific compliance requirements

Ongoing support and resources are available through the IT helpdesk and learning management system.""",
                    "subsections": []
                }
            ]
        }
        
        return policy_data