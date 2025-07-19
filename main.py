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
import requests  # NEW
import io        # NEW
from reportlab.lib.pagesizes import letter  # NEW
from reportlab.pdfgen import canvas         # NEW
from reportlab.lib.utils import ImageReader # NEW

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

# Linux-compatible wkhtmltopdf auto-discovery
PDFKIT_CONFIG = pdfkit.configuration()

SENDGRID_API_KEY = os.getenv("SENDGRID_API_KEY")
SENDER_EMAIL = os.getenv("SENDER_EMAIL")


def create_pdf_with_logo(text, output_path, recipient_email):
    c = canvas.Canvas(output_path, pagesize=letter)
    width, height = letter

    # Extract domain and fetch logo
    domain = recipient_email.split("@")[-1]
    logo_url = f"https://logo.clearbit.com/{domain}"

    try:
        response = requests.get(logo_url, timeout=5)
        if response.status_code == 200:
            logo_stream = io.BytesIO(response.content)
            logo = ImageReader(logo_stream)
            c.drawImage(logo, 72, height - 60, width=120, height=40)
    except Exception as e:
        print(f"Logo fetch failed: {e}")

    # Draw policy text
    text_object = c.beginText(72, height - 120)
    text_object.setFont("Helvetica", 11)
    for line in text.splitlines():
        text_object.textLine(line)
    c.drawText(text_object)
    c.save()


@app.post("/generate")
async def generate_pdf(request: Request):
    data = await request.json()

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
        # 🧠 Generate policy text
        policy_text = generate_policy_content(industry, company_size, compliance_target, use_case, tone)

        # 🖨 Create PDF with logo
        filename = f"{uuid.uuid4()}.pdf"
        filepath = f"./templates/{filename}"
        create_pdf_with_logo(policy_text, filepath, recipient_email)

        # 📨 Email with SendGrid
        with open(filepath, 'rb') as f:
            file_data = f.read()
            encoded = base64.b64encode(file_data).decode()

        attachment = Attachment(
            FileContent(encoded),
            FileName("policy.pdf"),
            FileType("application/pdf"),
            Disposition("attachment")
        )

        message = Mail(
            from_email=SENDER_EMAIL,
            to_emails=recipient_email,
            subject="Your CompliGenie Policy PDF",
            html_content="<p>Attached is your generated policy document.</p>"
        )
        message.attachment = attachment

        sg = SendGridAPIClient(SENDGRID_API_KEY)
        sg.send(message)

        return JSONResponse({"message": "Policy PDF generated and sent by email."})

    except Exception as e:
        print("EXCEPTION OCCURRED:")
        print(traceback.format_exc())
        return JSONResponse(status_code=500, content={"error": str(e)})
