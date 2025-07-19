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

# Windows wkhtmltopdf config
PDFKIT_CONFIG = pdfkit.configuration(
    wkhtmltopdf="C:\\Program Files\\wkhtmltopdf\\bin\\wkhtmltopdf.exe"
)

SENDGRID_API_KEY = os.getenv("SENDGRID_API_KEY")
SENDER_EMAIL = os.getenv("SENDER_EMAIL")

@app.post("/generate")
async def generate_pdf(request: Request):
    data = await request.json()

    industry = data.get("industry")
    company_size = data.get("company_size")
    compliance_target = data.get("compliance_target")
    use_case = data.get("use_case")
    tone = data.get("tone")
    recipient_email = data.get("recipient_email")  # added input

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

    filename = f"{uuid.uuid4()}.pdf"
    filepath = f"./templates/{filename}"
    pdfkit.from_string(html, filepath, configuration=PDFKIT_CONFIG)

    try:
        # Read + encode PDF
        with open(filepath, 'rb') as f:
            data = f.read()
            encoded = base64.b64encode(data).decode()

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
        return JSONResponse(status_code=500, content={"error": str(e)})
from fastapi.responses import RedirectResponse
from stripe_config import DOMAIN
import stripe  # Make sure this is imported at the top if not already

@app.post("/create-checkout-session")
async def create_checkout_session():
    try:
        checkout_session = stripe.checkout.Session.create(
            payment_method_types=["card"],
            line_items=[
                {
                    "price_data": {
                        "currency": "usd",
                        "product_data": {
                            "name": "AI Policy Document",
                        },
                        "unit_amount": 1000,  # $10.00 USD
                    },
                    "quantity": 1,
                }
            ],
            mode="payment",
            success_url=f"{DOMAIN}/success",
            cancel_url=f"{DOMAIN}/cancel",
        )
        return {"id": checkout_session.id}
    except Exception as e:
        return {"error": str(e)}
