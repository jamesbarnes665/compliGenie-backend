from fastapi import FastAPI, Request, HTTPException
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from dotenv import load_dotenv
import os
from pydantic import BaseModel
from typing import List, Optional

# Import your routes
from app.api import policies, partners  # Added partners

# Load environment variables
load_dotenv()

# Configure logging
import logging
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)
logger.info(f"STRIPE_SECRET_KEY loaded: {bool(os.getenv('STRIPE_SECRET_KEY'))}")

# Create request models
class PolicyRequest(BaseModel):
    company_name: str
    industry: str
    ai_tools: List[str]
    employee_count: int
    template_type: str = "standard"  # NEW FIELD
    recipient_email: Optional[str] = None

# Create FastAPI app
app = FastAPI(
    title="CompliGenie API",
    description="AI-powered policy generation platform for multi-partner integration",
    version="2.0.0"
)

# Configure CORS for frontend
app.add_middleware(
    CORSMiddleware,
    allow_origins=[
        "http://localhost:3000",  # Local frontend
        "https://compligenie.com",  # Production frontend
        "*"  # For development - remove in production
    ],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Include API routers
app.include_router(policies.router)
app.include_router(partners.router)  # Added partner routes

# Root endpoint
@app.get("/")
async def root():
    return {
        "message": "CompliGenie API is running",
        "version": "2.0.0",
        "endpoints": {
            "generate_policy": "/api/policies/generate",
            "partner_register": "/api/partners/register",
            "partner_dashboard": "/api/partners/dashboard",
            "health": "/health"
        }
    }

# Health check endpoint
@app.get("/health")
async def health_check():
    return {
        "status": "healthy",
        "service": "CompliGenie API",
        "timestamp": os.popen('date').read().strip()
    }

# Stripe webhook endpoint
@app.post("/webhooks/stripe")
async def stripe_webhook(request: Request):
    """Handle Stripe webhooks for payment and account updates"""
    from app.services.stripe_service import StripeConnectService
    from app.services.partner_store import partner_store
    
    payload = await request.body()
    sig_header = request.headers.get('stripe-signature')
    
    try:
        stripe_service = StripeConnectService()
        event = await stripe_service.verify_webhook(payload, sig_header)
        
        # Handle different event types
        if event['type'] == 'account.updated':
            # Partner completed onboarding
            account_id = event['data']['object']['id']
            charges_enabled = event['data']['object']['charges_enabled']
            
            # Find partner by stripe account ID and update
            for partner in partner_store.list_all():
                if partner.stripe_account_id == account_id:
                    partner.stripe_onboarding_completed = charges_enabled
                    partner_store.update(partner.id, partner.dict())
                    break
                    
        elif event['type'] == 'payment_intent.succeeded':
            # Payment successful - already handled by transfer_data
            pass
            
        elif event['type'] == 'transfer.created':
            # Transfer to partner created
            pass
            
        return {"status": "success"}
        
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))

# Global exception handler
@app.exception_handler(Exception)
async def global_exception_handler(request: Request, exc: Exception):
    return JSONResponse(
        status_code=500,
        content={
            "error": "Internal server error",
            "message": str(exc) if os.getenv("DEBUG") == "true" else "An error occurred processing your request"
        }
    )

# Startup event
@app.on_event("startup")
async def startup_event():
    print("ðŸš€ CompliGenie API starting up...")
    print(f"ðŸ“§ Email service: {'Enabled' if os.getenv('SENDGRID_API_KEY') else 'Disabled'}")
    print(f"ðŸ’³ Stripe Connect: {'Enabled' if os.getenv('STRIPE_SECRET_KEY') else 'Test Mode'}")
    print("âœ… Ready to serve requests")

# Shutdown event
@app.on_event("shutdown")
async def shutdown_event():
    print("ðŸ‘‹ CompliGenie API shutting down...")

# Legacy endpoint for backward compatibility (remove later)
@app.post("/generate")
async def legacy_generate(request: Request):
    """Legacy endpoint - redirects to new API structure"""
    data = await request.json()
    
    # Transform legacy data format to new format
    new_format = {
        "company_name": data.get("company_name", "Unknown Company"),
        "industry": data.get("industry", "Technology"),
        "employee_count": int(data.get("company_size", "50").split("-")[0]),
        "ai_tools": ["ChatGPT", "Claude"],  # Default tools
        "template_type": "standard",  # Default template
        "recipient_email": data.get("recipient_email")
    }
    
    # Redirect to new endpoint
    from app.api.policies import generate_policy
    return await generate_policy(PolicyRequest(**new_format))
