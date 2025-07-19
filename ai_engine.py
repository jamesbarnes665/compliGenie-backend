from langchain_community.chat_models import ChatOpenAI
from langchain.prompts import ChatPromptTemplate
from config import OPENAI_API_KEY

def get_regulatory_block(compliance_target: str) -> str:
    compliance_target = compliance_target.strip().lower()
    
    if "hipaa" in compliance_target:
        return """
Ensure that the policy addresses HIPAA-specific requirements for the use, disclosure, and safeguarding of Protected Health Information (PHI), including administrative, technical, and physical safeguards.
"""
    elif "gdpr" in compliance_target:
        return """
Ensure that the policy aligns with GDPR requirements including lawful basis for processing, data minimization, data subject rights (such as access and erasure), and the appointment of a Data Protection Officer if applicable.
"""
    elif "soc" in compliance_target:
        return """
Ensure that the policy follows SOC2 principles, especially regarding data security, integrity, confidentiality, and availability. Include controls and audits that demonstrate compliance.
"""
    elif "ccpa" in compliance_target:
        return """
Ensure that the policy reflects CCPA obligations including user data access rights, opt-out processes, and transparency in data collection, usage, and sale for California residents.
"""
    else:
        return ""  # Default: no regulatory block added

def generate_policy_content(industry, company_size, compliance_target, use_case, tone):
    llm = ChatOpenAI(temperature=0.2, openai_api_key=OPENAI_API_KEY, model="gpt-4")

    regulation_block = get_regulatory_block(compliance_target)

    template = """
You are a compliance officer writing an internal policy document for a {industry} company that is {company_size} in size.

Your task is to write a policy that addresses the following use case:
- {use_case}

The policy must comply with: {compliance_target}

Tone of the document should be: {tone}

{regulation_block}

Include the following sections:
1. Purpose
2. Scope
3. Policy Statement
4. Responsibilities
5. Enforcement
6. Review & Update Frequency

Format the output using section headers and structured paragraphs.
"""

    prompt = ChatPromptTemplate.from_template(template)
    chain = prompt | llm

    response = chain.invoke({
        "industry": industry,
        "company_size": company_size,
        "compliance_target": compliance_target,
        "use_case": use_case,
        "tone": tone,
        "regulation_block": regulation_block
    })

    return response.content.strip()
