from langchain.chat_models import ChatOpenAI
from langchain.prompts import ChatPromptTemplate
from config import OPENAI_API_KEY

def generate_policy_content(industry, company_size, compliance_target, use_case):
    llm = ChatOpenAI(temperature=0.2, openai_api_key=OPENAI_API_KEY, model="gpt-4")

    template = """
    You are a compliance officer writing an internal policy document.
    Write a professional, legally sound, and concise policy tailored to:
    - Industry: {industry}
    - Company Size: {company_size}
    - Compliance Requirement: {compliance_target}
    - Use Case: {use_case}

    The policy must include:
    1. Purpose
    2. Scope
    3. Policy Statement
    4. Responsibilities
    5. Enforcement
    6. Review & Update Frequency

    Format using clear section headers. Do not include disclaimers.
    """

    prompt = ChatPromptTemplate.from_template(template)
    chain = prompt | llm
    response = chain.invoke({
        "industry": industry,
        "company_size": company_size,
        "compliance_target": compliance_target,
        "use_case": use_case
    })

    return response.content.strip()
