from langchain.chat_models import ChatOpenAI
from langchain.prompts import ChatPromptTemplate
from config import OPENAI_API_KEY

def generate_policy_content(industry, company_size, compliance_target, use_case, tone):
    llm = ChatOpenAI(temperature=0.2, openai_api_key=OPENAI_API_KEY, model="gpt-4")

    template = """
    You are a compliance officer writing an internal policy document for a {industry} company that is {company_size} in size.

    Your task is to write a policy that addresses the following use case:
    - {use_case}

    The policy must comply with: {compliance_target}

    Tone of the document should be: {tone}

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
        "tone": tone
    })

    return response.content.strip()
