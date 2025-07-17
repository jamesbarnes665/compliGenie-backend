from fastapi import FastAPI, Request
from fastapi.responses import FileResponse
from fastapi.middleware.cors import CORSMiddleware
from ai_engine import generate_policy_content
import pdfkit
import uuid
import os

app = FastAPI()

# CORS configuration
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# PDFKit configuration for Windows
PDFKIT_CONFIG = pdfkit.configuration(
    wkhtmltopdf="C:\\Program Files\\wkhtmltopdf\\bin\\wkhtmltopdf.exe"  # update this if your path differs
)

@app.post("/generate")
async def generate_pdf(request: Request):
    data = await request.json()

    # Extract form input fields
    industry = data.get("industry")
    company_size = data.get("company_size")
    compliance_target = data.get("compliance_target")
    use_case = data.get("use_case")

    # Generate policy content from GPT-4
    policy_text = generate_policy_content(
        industry, company_size, compliance_target, use_case
    )

    # Build HTML for PDF
    html = f"""
    <html>
    <head><meta charset="UTF-8"></head>
    <body>
    <h1>Generated Policy Document</h1>
    <p><strong>Industry:</strong> {industry}</p>
    <p><strong>Company Size:</strong> {company_size}</p>
    <p><strong>Compliance Target:</strong> {compliance_target}</p>
    <p><strong>Use Case:</strong> {use_case}</p>
    <hr>
    <pre>{policy_text}</pre>
    </body>
    </html>
    """

    # Save as PDF to /templates/
    filename = f"{uuid.uuid4()}.pdf"
    filepath = f"./templates/{filename}"
    pdfkit.from_string(html, filepath, configuration=PDFKIT_CONFIG)

    return FileResponse(
        path=filepath,
        filename="policy.pdf",
        media_type="application/pdf"
    )
