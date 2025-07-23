# compligenie-backend/app/services/policy_generator.py

from typing import Dict, Any, List
from datetime import datetime
import json

class PolicyGenerator:
    """AI Policy Generator Service"""
    
    def __init__(self):
        self.policy_templates = {
            "technology": "Technology Company AI Usage Policy",
            "healthcare": "Healthcare AI Compliance Policy",
            "finance": "Financial Services AI Policy",
            "retail": "Retail AI Implementation Policy",
            "default": "General AI Usage Policy"
        }
    
    def generate_policy(self, request_data: Dict[str, Any]) -> Dict[str, Any]:
        """
        Generate an AI compliance policy based on company details
        """
        company_name = request_data.get("company_name", "Company")
        industry = request_data.get("industry", "default")
        state = request_data.get("state", "California")
        ai_tools = request_data.get("ai_tools", [])
        employee_count = request_data.get("employee_count", 0)
        
        # Select appropriate policy template
        policy_type = self.policy_templates.get(industry, self.policy_templates["default"])
        
        # Generate policy content
        policy_content = {
            "policy_type": policy_type,
            "company_name": company_name,
            "effective_date": datetime.utcnow().strftime("%B %d, %Y"),
            "overview": f"""
                This AI Usage Policy governs the use of artificial intelligence tools and systems 
                at {company_name}. As a {industry} company with {employee_count} employees, 
                we are committed to the responsible and ethical use of AI technologies including 
                {', '.join(ai_tools) if ai_tools else 'various AI tools'}.
            """,
            "purpose": f"""
                The purpose of this policy is to establish guidelines for the safe, ethical, and 
                compliant use of AI technologies at {company_name}, ensuring alignment with 
                {state} state regulations and industry best practices.
            """,
            "scope": f"""
                This policy applies to all employees, contractors, and third parties who use AI 
                tools on behalf of {company_name}. It covers the use of AI tools including but 
                not limited to: {', '.join(ai_tools) if ai_tools else 'ChatGPT, Claude, Copilot, and other AI systems'}.
            """,
            "guidelines": self._generate_guidelines(industry, ai_tools),
            "compliance": self._generate_compliance_requirements(state, industry),
            "data_privacy": self._generate_data_privacy_section(industry),
            "accountability": f"""
                All employees using AI tools must:
                - Complete AI ethics training
                - Document AI usage for compliance purposes
                - Report any concerns or violations to their supervisor
                - Regularly review and acknowledge this policy
            """,
            "violations": """
                Violations of this policy may result in:
                - Mandatory retraining
                - Suspension of AI tool access
                - Disciplinary action up to and including termination
                - Legal action if warranted
            """,
            "review_schedule": "This policy will be reviewed and updated quarterly or as needed based on regulatory changes.",
            "approval": {
                "approved_by": "Compliance Department",
                "approval_date": datetime.utcnow().strftime("%B %d, %Y")
            }
        }
        
        return policy_content
    
    def _generate_guidelines(self, industry: str, ai_tools: List[str]) -> str:
        """Generate industry-specific guidelines"""
        base_guidelines = """
        1. Always verify AI-generated content for accuracy
        2. Never share confidential or proprietary information with AI tools
        3. Maintain human oversight for all AI-assisted decisions
        4. Document the use of AI in critical processes
        5. Ensure AI usage complies with all applicable laws and regulations
        """
        
        industry_specific = {
            "healthcare": """
        6. Never input patient health information (PHI) into AI systems
        7. Ensure HIPAA compliance in all AI interactions
        8. Validate all medical advice or diagnoses with qualified professionals
            """,
            "finance": """
        6. Comply with SOX, GDPR, and financial regulations
        7. Never use AI for insider trading or market manipulation
        8. Ensure AI decisions in lending comply with fair lending laws
            """,
            "technology": """
        6. Protect source code and technical specifications
        7. Review AI-generated code for security vulnerabilities
        8. Ensure AI usage doesn't violate software licenses
            """
        }
        
        return base_guidelines + industry_specific.get(industry, "")
    
    def _generate_compliance_requirements(self, state: str, industry: str) -> str:
        """Generate compliance requirements based on state and industry"""
        requirements = f"""
        Compliance with {state} State Regulations:
        - Data privacy laws and consumer protection regulations
        - AI transparency and explainability requirements
        - Non-discrimination and bias prevention measures
        """
        
        if state == "California":
            requirements += """
        - California Consumer Privacy Act (CCPA) compliance
        - SB 1001 (Bot Disclosure Law) compliance
        - Upcoming AI regulation compliance
            """
        
        return requirements
    
    def _generate_data_privacy_section(self, industry: str) -> str:
        """Generate data privacy guidelines"""
        return f"""
        Data Privacy and Security:
        1. Never input personally identifiable information (PII) into public AI tools
        2. Use only approved AI tools that meet our security standards
        3. Ensure data retention policies are followed
        4. Report any data breaches immediately
        5. Maintain audit logs of AI tool usage
        {'6. Ensure HIPAA compliance for all patient data' if industry == 'healthcare' else ''}
        {'6. Ensure PCI DSS compliance for payment data' if industry == 'finance' else ''}
        """