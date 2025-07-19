from fastapi import FastAPI, Request
from fastapi.responses import FileResponse, JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from ai_engine import generate_policy_content
import pdfkit
import uuid
import os
from sendgrid import SendGridAPIClient
from sendgrid.helpers.mail import Mail, Attachment, FileContent, FileName, FileType, Disposition
import base64
from dotenv import load_dotenv
import traceback

load_dotenv()

app = FastAPI()

# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Linux-compatible wkhtmltopdf auto-discovery (no hardcoded Windows path)
PDFKIT_CONFIG = pdfkit.configuration()

# Load environment variables
SENDGRID_API_KEY = os.getenv("SENDGRID_API_KEY")
SENDER_EMAIL = os.getenv("SENDER_EMAIL")


@app.post("/generate")
async def generate_pdf(request: Request):
    data = await request.json()

    # ✅ Debugging output to Render logs
    print("DATA RECEIVED:", data)
    print("SENDGRID_API_KEY:", SENDGRID_API_KEY)
    print("SENDER_EMAIL:", SENDER_EMAIL)

    industry = data.get("industry")
    company_size = data.get("company_size")
    compliance_target = data.get("compliance_target")
    use_case = data.get("use_case")
    tone = data.get("tone")
    recipient_email = data.get("recipient_email")

    try:
        # Generate policy text
        policy_text = generate_policy_content(industry, company_size, compliance_target, use_case, tone)

        # HTML template
        html = f"""
        <html><body>
        <h1>Generated Policy Document</h1>
        <p><strong>Industry:</strong> {industry}</p>
        <p><strong>Company Size:</strong> {company_size}</p>
        <p><strong>Compliance Target:</strong> {compliance_target}</p>
        <p><strong>Use Case:</strong> {use_case}</p>
        <p><strong>Tone:</strong> {tone}</p>
        <hr>
        <pre>{policy_text}</pre>
        </body></html>
        """

        # Save to local file
        filename = f"{uuid.uuid4()}.pdf"
        filepath = f"./templates/{filename}"
        pdfkit.from_string(html, filepath, configuration=PDFKIT_CONFIG)

        # Read and encode PDF
        with open(filepath, 'rb') as f:
            file_data = f.read()
            encoded = base64.b64encode(file_data).decode()

        attachment = Attachment(
            FileContent(encoded),
            FileName("policy.pdf"),
            FileType("application/pdf"),
            Disposition("attachment")
        )

        # Build email
        message = Mail(
            from_email=SENDER_EMAIL,
            to_emails=recipient_email,
            subject="Your CompliGenie Policy PDF",
            html_content="<p>Attached is your generated policy document.</p>"
        )
        message.attachment = attachment

        sg = SendGridAPIClient(SENDGRID_API_KEY)
        sg.send(message)

        return JSONResponse({"message
