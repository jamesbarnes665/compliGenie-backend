from fastapi import FastAPI, Form
from fastapi.responses import FileResponse
from langchain.chat_models import ChatOpenAI
from langchain.prompts import PromptTemplate
from config import OPENAI_API_KEY
from fpdf import FPDF
import os

os.environ["OPENAI_API_KEY"] = OPENAI_API_KEY

app = FastAPI()
llm = ChatOpenAI(model_name="gpt-4")

prompt_template = PromptTemplate(
    input_variables=["industry", "company_size", "ai_use_case", "compliance_target"],
    template=open("templates/prompt.txt").read()
)

@app.post("/generate-policy-pdf")
def generate_policy_pdf(
    industry: str = Form(...),
    company_size: str = Form(...),
    ai_use_case: str = Form(...),
    compliance_target: str = Form(...)
):
    prompt = prompt_template.format(
        industry=industry,
        company_size=company_size,
        ai_use_case=ai_use_case,
        compliance_target=compliance_target
    )
    policy = llm.predict(prompt)

    pdf = FPDF()
    pdf.add_page()
    pdf.set_auto_page_break(auto=True, margin=15)
    pdf.set_font("Arial", size=12)
    for line in policy.split('\n'):
        pdf.multi_cell(0, 10, line)

    output_path = f"output/{industry}_policy.pdf"
    pdf.output(output_path)

    return FileResponse(output_path, media_type='application/pdf', filename="AI_Usage_Policy.pdf")
