# compligenie-backend/app/services/legal_policy_generator.py

from typing import Dict, Any
from datetime import datetime

class LegalPolicyGenerator:
    """Specialized policy generator for legal industry"""
    
    def generate_legal_policy(self, request_data: Dict[str, Any]) -> Dict[str, Any]:
        """Generate legal industry-specific AI policy"""
        
        firm_name = request_data.get("company_name", "Law Firm")
        state = request_data.get("state", "California")
        employee_count = request_data.get("employee_count", 0)
        practice_areas = request_data.get("practice_areas", ["General Practice"])
        ai_tools = request_data.get("ai_tools", [])
        
        policy_content = {
            "policy_type": "Legal Industry AI Usage Policy",
            "company_name": firm_name,
            "effective_date": datetime.utcnow().strftime("%B %d, %Y"),
            "overview": f"""
                This AI Usage Policy governs the use of artificial intelligence tools and systems 
                at {firm_name}. As a legal services provider, we maintain the highest standards 
                of confidentiality, accuracy, and ethical conduct in our use of AI technologies.
                This policy ensures compliance with legal ethics rules, client confidentiality 
                requirements, and professional responsibility standards.
            """,
            "legal_specific_guidelines": f"""
                CRITICAL LEGAL INDUSTRY REQUIREMENTS:
                
                1. CLIENT CONFIDENTIALITY
                   - NEVER input client names, case details, or any confidential information into AI tools
                   - All client data must remain within approved, secure systems
                   - Violation of client confidentiality may result in immediate termination and bar discipline
                
                2. ATTORNEY-CLIENT PRIVILEGE
                   - AI tools must not be used in ways that could waive privilege
                   - No privileged communications may be shared with AI systems
                   - Document all AI usage that relates to client matters
                
                3. PROFESSIONAL RESPONSIBILITY
                   - Attorneys remain fully responsible for all AI-generated content
                   - AI output must be thoroughly reviewed and verified
                   - Cannot rely solely on AI for legal analysis or advice
                
                4. ETHICAL OBLIGATIONS
                   - Duty of competence requires understanding AI limitations
                   - Duty of supervision extends to AI tool usage
                   - Billing practices must accurately reflect AI assistance
            """,
            "approved_uses": """
                PERMITTED AI USES IN LEGAL PRACTICE:
                
                ✓ Legal research on public law and regulations
                ✓ Initial draft templates for standard documents
                ✓ Grammar and style checking (without client data)
                ✓ Administrative task automation
                ✓ Public legal information summaries
                ✓ Marketing content creation
                ✓ Internal knowledge management (non-confidential)
            """,
            "prohibited_uses": """
                STRICTLY PROHIBITED AI USES:
                
                ✗ Inputting any client confidential information
                ✗ Uploading case files or privileged documents
                ✗ Generating legal advice without attorney review
                ✗ Court filing preparation without thorough verification
                ✗ Contract analysis with actual client contracts
                ✗ Discovery document review with real case materials
                ✗ Legal strategy development for specific cases
            """,
            "compliance": f"""
                COMPLIANCE WITH LEGAL REGULATIONS:
                
                State Bar Requirements ({state}):
                - Maintain competence in technology use
                - Protect client confidentiality
                - Avoid unauthorized practice of law
                - Ensure reasonable fees when using AI assistance
                
                Data Security Requirements:
                - Use only approved AI tools vetted by IT
                - Enable maximum privacy settings
                - Maintain audit logs of AI usage
                - Report any potential data breaches immediately
                
                Malpractice Prevention:
                - Document AI tool usage in client matters
                - Maintain professional liability coverage
                - Implement quality control procedures
                - Regular training on AI limitations
            """,
            "implementation": f"""
                IMPLEMENTATION FOR {firm_name}:
                
                1. All attorneys and staff must complete AI ethics training
                2. IT department maintains list of approved AI tools
                3. Monthly audits of AI usage compliance
                4. Client engagement letters include AI usage disclosure
                5. Billing practices clearly indicate AI-assisted work
                
                Firm Size Considerations ({employee_count} employees):
                {"- Designate AI compliance officer" if employee_count > 20 else "- Managing partner oversees AI compliance"}
                {"- Department-specific AI guidelines" if employee_count > 50 else "- Firm-wide AI guidelines"}
                {"- Regular compliance audits" if employee_count > 10 else "- Quarterly compliance reviews"}
            """
        }
        
        return policy_content