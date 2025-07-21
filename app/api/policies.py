# backend/app/api/policies.py

from fastapi import APIRouter, HTTPException
from fastapi.responses import StreamingResponse
from pydantic import BaseModel
from typing import List, Optional
import traceback

# Import your services
from app.services.pdf_generator import EnhancedPDFGenerator
from app.services.policy_generator import PolicyGenerator

# Create the router
router = APIRouter(prefix="/api/policies", tags=["policies"])

# Request model
class PolicyGenerationRequest(BaseModel):
    company_name: str
    industry: str
    ai_tools: List[str]
    employee_count: int
    partner_id: Optional[str] = None

@router.post("/generate")
async def generate_policy(request: PolicyGenerationRequest):
    """Generate AI policy with enhanced PDF formatting"""
    try:
        print(f"Generating policy for {request.company_name}")
        
        # Generate policy content
        policy_generator = PolicyGenerator()
        policy_content = await policy_generator.generate(
            company_name=request.company_name,
            industry=request.industry,
            ai_tools=request.ai_tools,
            employee_count=request.employee_count
        )
        
        # Create PDF with enhanced formatting
        pdf_generator = EnhancedPDFGenerator()
        
        # Apply partner branding if this is a partner request
        partner_branding = None
        if request.partner_id:
            # TODO: Implement partner branding lookup
            pass
        
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

@router.get("/test")
async def test_endpoint():
    """Test endpoint to verify API is working"""
    return {"message": "Policies API is working"}