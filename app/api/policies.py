# backend/app/api/policies.py

from fastapi import APIRouter, HTTPException
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

# Industry-specific template configurations
INDUSTRY_TEMPLATES = {
    "healthcare": {
        "compliance_frameworks": ["HIPAA", "HITECH", "FDA"],
        "data_sensitivity": "high",
        "specific_sections": {
            "patient_data": True,
            "clinical_decision_support": True,
            "medical_device_integration": True
        },
        "risk_level": "critical",
        "audit_frequency": "quarterly"
    },
    "finance": {
        "compliance_frameworks": ["SOX", "GLBA", "GDPR", "CCPA"],
        "data_sensitivity": "high",
        "specific_sections": {
            "financial_data": True,
            "algorithmic_trading": True,
            "fraud_detection": True
        },
        "risk_level": "critical",
        "audit_frequency": "monthly"
    },
    "education": {
        "compliance_frameworks": ["FERPA", "COPPA", "GDPR"],
        "data_sensitivity": "medium",
        "specific_sections": {
            "student_records": True,
            "academic_integrity": True,
            "parental_consent": True
        },
        "risk_level": "high",
        "audit_frequency": "semi-annual"
    },
    "retail": {
        "compliance_frameworks": ["PCI-DSS", "GDPR", "CCPA"],
        "data_sensitivity": "medium",
        "specific_sections": {
            "customer_data": True,
            "recommendation_systems": True,
            "inventory_management": True
        },
        "risk_level": "medium",
        "audit_frequency": "quarterly"
    },
    "manufacturing": {
        "compliance_frameworks": ["ISO 9001", "OSHA", "EPA"],
        "data_sensitivity": "medium",
        "specific_sections": {
            "production_data": True,
            "quality_control": True,
            "supply_chain": True
        },
        "risk_level": "medium",
        "audit_frequency": "quarterly"
    },
    "technology": {
        "compliance_frameworks": ["SOC 2", "ISO 27001", "GDPR"],
        "data_sensitivity": "high",
        "specific_sections": {
            "source_code": True,
            "api_usage": True,
            "development_practices": True
        },
        "risk_level": "high",
        "audit_frequency": "quarterly"
    },
    "legal": {
        "compliance_frameworks": ["ABA Model Rules", "GDPR", "Attorney-Client Privilege"],
        "data_sensitivity": "critical",
        "specific_sections": {
            "client_confidentiality": True,
            "document_review": True,
            "legal_research": True
        },
        "risk_level": "critical",
        "audit_frequency": "monthly"
    },
    "government": {
        "compliance_frameworks": ["FISMA", "FedRAMP", "NIST"],
        "data_sensitivity": "critical",
        "specific_sections": {
            "classified_information": True,
            "citizen_services": True,
            "transparency_requirements": True
        },
        "risk_level": "critical",
        "audit_frequency": "monthly"
    }
}

# Request models
class PolicyGenerationRequest(BaseModel):
    company_name: str
    industry: str
    ai_tools: List[str]
    employee_count: int
    partner_id: Optional[str] = None
    template_customizations: Optional[Dict] = None

class IndustryTemplateResponse(BaseModel):
    industry: str
    template: Dict
    available_customizations: List[str]

# Enhanced request model with industry-specific options
class EnhancedPolicyRequest(PolicyGenerationRequest):
    compliance_priority: Optional[str] = "balanced"  # balanced, strict, flexible
    include_industry_benchmarks: Optional[bool] = True
    custom_compliance_frameworks: Optional[List[str]] = None
    risk_tolerance: Optional[str] = "medium"  # low, medium, high

@router.post("/generate")
async def generate_policy(request: PolicyGenerationRequest):
    """Generate AI policy with industry-specific templates"""
    try:
        print(f"Generating policy for {request.company_name} in {request.industry}")
        
        # Get industry template
        industry_template = INDUSTRY_TEMPLATES.get(
            request.industry.lower(), 
            get_default_template()
        )
        
        # Merge with any custom templates
        if request.template_customizations:
            industry_template = merge_templates(
                industry_template, 
                request.template_customizations
            )
        
        # Generate policy content with industry focus
        policy_generator = PolicyGenerator()
        policy_content = await policy_generator.generate(
            company_name=request.company_name,
            industry=request.industry,
            ai_tools=request.ai_tools,
            employee_count=request.employee_count,
            industry_template=industry_template
        )
        
        # Create PDF with enhanced formatting
        pdf_generator = EnhancedPDFGenerator()
        
        # Apply partner branding if this is a partner request
        partner_branding = None
        if request.partner_id:
            partner_branding = await get_partner_branding(request.partner_id)
        
        # Generate PDF without metadata parameter
        pdf_buffer = pdf_generator.generate_policy_pdf(
            policy_content,
            partner_branding=partner_branding
        )
        
        # Return PDF as streaming response
        return StreamingResponse(
            pdf_buffer,
            media_type="application/pdf",
            headers={
                "Content-Disposition": f"attachment; filename=ai_policy_{request.company_name.replace(' ', '_')}_{request.industry}.pdf"
            }
        )
        
    except Exception as e:
        print(f"Error generating policy: {str(e)}")
        print(traceback.format_exc())
        raise HTTPException(status_code=500, detail=str(e))

@router.post("/generate/enhanced")
async def generate_enhanced_policy(request: EnhancedPolicyRequest):
    """Generate AI policy with advanced industry customizations"""
    try:
        # Get base industry template
        industry_template = INDUSTRY_TEMPLATES.get(
            request.industry.lower(), 
            get_default_template()
        )
        
        # Apply enhanced customizations
        if request.custom_compliance_frameworks:
            industry_template["compliance_frameworks"].extend(
                request.custom_compliance_frameworks
            )
        
        # Adjust based on compliance priority
        if request.compliance_priority == "strict":
            industry_template["audit_frequency"] = "monthly"
            industry_template["risk_level"] = "critical"
        elif request.compliance_priority == "flexible":
            industry_template["audit_frequency"] = "annual"
        
        # Generate policy with enhanced options
        policy_generator = PolicyGenerator()
        policy_content = await policy_generator.generate_enhanced(
            company_name=request.company_name,
            industry=request.industry,
            ai_tools=request.ai_tools,
            employee_count=request.employee_count,
            industry_template=industry_template,
            compliance_priority=request.compliance_priority,
            include_benchmarks=request.include_industry_benchmarks,
            risk_tolerance=request.risk_tolerance
        )
        
        # Create PDF
        pdf_generator = EnhancedPDFGenerator()
        partner_branding = None
        if request.partner_id:
            partner_branding = await get_partner_branding(request.partner_id)
        
        pdf_buffer = pdf_generator.generate_policy_pdf(
            policy_content,
            partner_branding=partner_branding
        )
        
        return StreamingResponse(
            pdf_buffer,
            media_type="application/pdf",
            headers={
                "Content-Disposition": f"attachment; filename=ai_policy_{request.company_name.replace(' ', '_')}_{request.industry}_enhanced.pdf"
            }
        )
        
    except Exception as e:
        print(f"Error generating enhanced policy: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/templates/{industry}")
async def get_industry_template(industry: str):
    """Get available template for a specific industry"""
    template = INDUSTRY_TEMPLATES.get(industry.lower())
    
    if not template:
        raise HTTPException(
            status_code=404, 
            detail=f"No template found for industry: {industry}"
        )
    
    return IndustryTemplateResponse(
        industry=industry,
        template=template,
        available_customizations=[
            "compliance_frameworks",
            "data_sensitivity",
            "specific_sections",
            "risk_level",
            "audit_frequency"
        ]
    )

@router.get("/templates")
async def list_industry_templates():
    """List all available industry templates"""
    return {
        "industries": list(INDUSTRY_TEMPLATES.keys()),
        "templates": INDUSTRY_TEMPLATES,
        "customization_options": {
            "compliance_priority": ["balanced", "strict", "flexible"],
            "risk_tolerance": ["low", "medium", "high"],
            "data_sensitivity": ["low", "medium", "high", "critical"]
        }
    }

@router.post("/templates/preview")
async def preview_policy_sections(request: PolicyGenerationRequest):
    """Preview the sections that will be included based on industry"""
    try:
        industry_template = INDUSTRY_TEMPLATES.get(
            request.industry.lower(), 
            get_default_template()
        )
        
        # Generate section preview
        policy_generator = PolicyGenerator()
        sections_preview = await policy_generator.preview_sections(
            industry=request.industry,
            ai_tools=request.ai_tools,
            industry_template=industry_template
        )
        
        return {
            "industry": request.industry,
            "sections": sections_preview,
            "compliance_frameworks": industry_template.get("compliance_frameworks", []),
            "estimated_pages": estimate_page_count(sections_preview, request.ai_tools)
        }
        
    except Exception as e:
        print(f"Error previewing sections: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/test")
async def test_endpoint():
    """Test endpoint to verify API is working"""
    return {
        "message": "Policies API is working",
        "available_industries": list(INDUSTRY_TEMPLATES.keys()),
        "version": "1.19f"
    }

# Helper functions
def get_default_template() -> Dict:
    """Return default template for unknown industries"""
    return {
        "compliance_frameworks": ["GDPR", "CCPA"],
        "data_sensitivity": "medium",
        "specific_sections": {
            "general_usage": True,
            "data_protection": True,
            "ethical_guidelines": True
        },
        "risk_level": "medium",
        "audit_frequency": "semi-annual"
    }

def merge_templates(base_template: Dict, customizations: Dict) -> Dict:
    """Merge custom template settings with base template"""
    merged = base_template.copy()
    
    for key, value in customizations.items():
        if key in merged:
            if isinstance(value, list) and isinstance(merged[key], list):
                merged[key] = list(set(merged[key] + value))
            elif isinstance(value, dict) and isinstance(merged[key], dict):
                merged[key].update(value)
            else:
                merged[key] = value
        else:
            merged[key] = value
    
    return merged

async def get_partner_branding(partner_id: str) -> Optional[Dict]:
    """Retrieve partner branding configuration"""
    # TODO: Implement actual partner branding lookup
    # This would typically query a database or configuration service
    partner_configs = {
        "partner_001": {
            "logo_url": "/assets/partners/partner_001_logo.png",
            "primary_color": "#003366",
            "secondary_color": "#0066CC",
            "company_name": "TechPartner Solutions"
        },
        "partner_002": {
            "logo_url": "/assets/partners/partner_002_logo.png",
            "primary_color": "#2E7D32",
            "secondary_color": "#66BB6A",
            "company_name": "GreenTech Innovations"
        }
    }
    
    return partner_configs.get(partner_id)

def estimate_page_count(sections: List[str], ai_tools: List[str]) -> int:
    """Estimate PDF page count based on sections and tools"""
    base_pages = 5  # Cover, TOC, Introduction, Conclusion, Appendix
    section_pages = len(sections) * 2  # Average 2 pages per section
    tool_pages = len(ai_tools) * 0.5  # Half page per AI tool
    
    return int(base_pages + section_pages + tool_pages)