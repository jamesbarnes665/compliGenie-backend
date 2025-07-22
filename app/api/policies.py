# backend/app/api/policies.py

from fastapi import APIRouter, HTTPException, Header
from fastapi.responses import StreamingResponse
from pydantic import BaseModel
from typing import List, Optional, Dict
import traceback
from datetime import datetime

# Import your services
from app.services.pdf_generator import EnhancedPDFGenerator
from app.services.policy_generator import PolicyGenerator

# Create the router
router = APIRouter(prefix="/api/policies", tags=["policies"])

# Request models
class PolicyGenerationRequest(BaseModel):
    company_name: str
    industry: str
    state: str
    ai_tools: List[str]
    employee_count: int
    company_size: Optional[str] = None
    data_types: Optional[List[str]] = None
    compliance_requirements: Optional[List[str]] = None
    partner_id: Optional[str] = None
    partner_branding: Optional[Dict] = None
    template_customizations: Optional[Dict] = None

@router.post("/generate")
async def generate_policy(
    request: PolicyGenerationRequest,
    x_partner_id: Optional[str] = Header(None)
):
    """Generate AI policy with industry-specific templates"""
    try:
        # Get partner context from headers
        partner_id = x_partner_id or request.partner_id
        
        print(f"Generating policy for {request.company_name} in {request.industry}")
        if partner_id:
            print(f"Partner ID: {partner_id}")
        
        # Generate policy content
        policy_generator = PolicyGenerator()
        policy_content = await policy_generator.generate(
            company_name=request.company_name,
            industry=request.industry,
            ai_tools=request.ai_tools,
            employee_count=request.employee_count,
            industry_template={}
        )
        
        # Add state to policy content
        policy_content["state"] = request.state
        
        # Create PDF
        pdf_generator = EnhancedPDFGenerator()
        
        # Apply partner branding if available
        partner_branding = request.partner_branding
        if partner_id and not partner_branding:
            # Default partner branding
            partner_branding = {
                "primaryColor": "#007bff",
                "secondaryColor": "#6c757d"
            }
        
        # Generate PDF
        pdf_buffer = pdf_generator.generate_policy_pdf(
            policy_content,
            partner_branding=partner_branding
        )
        
        # Return PDF as streaming response
        return StreamingResponse(
            pdf_buffer,
            media_type="application/pdf",
            headers={
                "Content-Disposition": f"attachment; filename=ai_policy_{request.company_name.replace(' ', '_')}.pdf"
            }
        )
        
    except Exception as e:
        print(f"Error generating policy: {str(e)}")
        print(traceback.format_exc())
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/compliance-requirements")
async def get_compliance_requirements(industry: str, state: str):
    """Get compliance requirements for industry and state"""
    try:
        return {
            "industry": industry,
            "state": state,
            "requirements": [
                "GDPR Compliance",
                "CCPA Compliance",
                "Industry Standards"
            ]
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/test")
async def test_endpoint():
    """Test endpoint to verify API is working"""
    return {
        "message": "Policies API is working",
        "version": "1.0"
    }

@router.get("/test-partner/{partner_id}")
async def test_partner_endpoint(partner_id: str):
    """Test partner endpoint"""
    return {
        "message": "Partner endpoint working",
        "partner_id": partner_id,
        "backend": "Python"
    }