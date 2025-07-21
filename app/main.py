from fastapi import FastAPI, Request, HTTPException
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
from dotenv import load_dotenv
import os
from pydantic import BaseModel
from typing import List, Optional

# Import your routes
from app.api import policies

# Load environment variables
load_dotenv()

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

# Root endpoint
@app.get("/")
async def root():
    return {
        "message": "CompliGenie API is running",
        "version": "2.0.0",
        "endpoints": {
            "generate_policy": "/api/policies/generate",
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
    print("🚀 CompliGenie API starting up...")
    print(f"📧 Email service: {'Enabled' if os.getenv('SENDGRID_API_KEY') else 'Disabled'}")
    print("✅ Ready to serve requests")

# Shutdown event
@app.on_event("shutdown")
async def shutdown_event():
    print("👋 CompliGenie API shutting down...")

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