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
        "audit_frequency": "quarterly",
        "ai_compliance": {
            "transparency_level": "high",
            "bias_testing_frequency": "monthly",
            "audit_retention": "6 years"
        }
    },
    "finance": {
        "compliance_frameworks": ["SOX", "GLBA", "GDPR", "CCPA", "FCRA", "ECOA"],
        "data_sensitivity": "high",
        "specific_sections": {
            "financial_data": True,
            "algorithmic_trading": True,
            "fraud_detection": True
        },
        "risk_level": "critical",
        "audit_frequency": "monthly",
        "ai_compliance": {
            "transparency_level": "high",
            "bias_testing_frequency": "monthly",
            "audit_retention": "7 years"
        }
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
        "audit_frequency": "semi-annual",
        "ai_compliance": {
            "transparency_level": "medium",
            "bias_testing_frequency": "quarterly",
            "audit_retention": "3 years"
        }
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
        "audit_frequency": "quarterly",
        "ai_compliance": {
            "transparency_level": "medium",
            "bias_testing_frequency": "quarterly",
            "audit_retention": "3 years"
        }
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
        "audit_frequency": "quarterly",
        "ai_compliance": {
            "transparency_level": "medium",
            "bias_testing_frequency": "semi-annual",
            "audit_retention": "3 years"
        }
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
        "audit_frequency": "quarterly",
        "ai_compliance": {
            "transparency_level": "high",
            "bias_testing_frequency": "quarterly",
            "audit_retention": "3 years"
        }
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
        "audit_frequency": "monthly",
        "ai_compliance": {
            "transparency_level": "critical",
            "bias_testing_frequency": "monthly",
            "audit_retention": "7 years"
        }
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
        "audit_frequency": "monthly",
        "ai_compliance": {
            "transparency_level": "critical",
            "bias_testing_frequency": "monthly",
            "audit_retention": "7 years"
        }
    },
    "insurance": {
        "compliance_frameworks": ["NAIC", "GDPR", "State Insurance Regulations"],
        "data_sensitivity": "high",
        "specific_sections": {
            "underwriting_decisions": True,
            "claims_processing": True,
            "actuarial_models": True
        },
        "risk_level": "high",
        "audit_frequency": "quarterly",
        "ai_compliance": {
            "transparency_level": "high",
            "bias_testing_frequency": "monthly",
            "audit_retention": "5 years"
        }
    }
}

# State-specific compliance requirements
STATE_COMPLIANCE = {
    "CA": {
        "frameworks": ["CCPA", "SB 1001", "Unruh Act"],
        "ai_specific": ["Bot disclosure", "Automated decision opt-out"],
        "retention_period": "4 years"
    },
    "NY": {
        "frameworks": ["SHIELD Act", "DFS Cybersecurity"],
        "ai_specific": ["NYC Local Law 144", "Bias audit requirements"],
        "retention_period": "6 years"
    },
    "IL": {
        "frameworks": ["BIPA"],
        "ai_specific": ["Biometric data consent", "AI notification"],
        "retention_period": "3 years"
    },
    "CO": {
        "frameworks": ["CPA"],
        "ai_specific": ["Profiling opt-out", "AI impact assessments"],
        "retention_period": "3 years"
    },
    "WA": {
        "frameworks": ["WPA"],
        "ai_specific": ["Algorithmic accountability", "AI transparency"],
        "retention_period": "3 years"
    }
}

# Request models
class PolicyGenerationRequest(BaseModel):
    company_name: str
    industry: str
    state: str  # Added state field
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
    include_ai_compliance: Optional[bool] = True  # New field for AI compliance

# New models for AI compliance
class ComplianceRequirementsRequest(BaseModel):
    industry: str
    state: str

class ComplianceRequirementsResponse(BaseModel):
    industry_compliance: Dict
    state_requirements: Dict
    ai_transparency_level: str
    bias_testing_frequency: str
    audit_retention_period: str

@router.post("/generate")
async def generate_policy(request: PolicyGenerationRequest):
    """Generate AI policy with industry-specific templates and AI compliance sections"""
    try:
        print(f"Generating policy for {request.company_name} in {request.industry}, {request.state}")
        
        # Get industry template
        industry_template = INDUSTRY_TEMPLATES.get(
            request.industry.lower(), 
            get_default_template()
        )
        
        # Get state compliance requirements
        state_compliance = STATE_COMPLIANCE.get(
            request.state.upper(),
            get_default_state_compliance()
        )
        
        # Merge state requirements into template
        industry_template["state_compliance"] = state_compliance
        
        # Merge with any custom templates
        if request.template_customizations:
            industry_template = merge_templates(
                industry_template, 
                request.template_customizations
            )
        
        # Generate policy content with industry focus and AI compliance
        policy_generator = PolicyGenerator()
        policy_content = await policy_generator.generate(
            company_name=request.company_name,
            industry=request.industry,
            ai_tools=request.ai_tools,
            employee_count=request.employee_count,
            industry_template=industry_template
        )
        
        # Add state to policy content for PDF generation
        policy_content["state"] = request.state
        
        # Create PDF with enhanced formatting
        pdf_generator = EnhancedPDFGenerator()
        
        # Apply partner branding if this is a partner request
        partner_branding = None
        if request.partner_id:
            partner_branding = await get_partner_branding(request.partner_id)
        
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
                "Content-Disposition": f"attachment; filename=ai_policy_{request.company_name.replace(' ', '_')}_{request.industry}_{request.state}.pdf"
            }
        )
        
    except Exception as e:
        print(f"Error generating policy: {str(e)}")
        print(traceback.format_exc())
        raise HTTPException(status_code=500, detail=str(e))

@router.post("/generate/enhanced")
async def generate_enhanced_policy(request: EnhancedPolicyRequest):
    """Generate AI policy with advanced industry customizations and AI compliance"""
    try:
        # Get base industry template
        industry_template = INDUSTRY_TEMPLATES.get(
            request.industry.lower(), 
            get_default_template()
        )
        
        # Get state compliance
        state_compliance = STATE_COMPLIANCE.get(
            request.state.upper(),
            get_default_state_compliance()
        )
        
        industry_template["state_compliance"] = state_compliance
        
        # Apply enhanced customizations
        if request.custom_compliance_frameworks:
            industry_template["compliance_frameworks"].extend(
                request.custom_compliance_frameworks
            )
        
        # Adjust based on compliance priority
        if request.compliance_priority == "strict":
            industry_template["audit_frequency"] = "monthly"
            industry_template["risk_level"] = "critical"
            industry_template["ai_compliance"]["bias_testing_frequency"] = "weekly"
        elif request.compliance_priority == "flexible":
            industry_template["audit_frequency"] = "annual"
            industry_template["ai_compliance"]["bias_testing_frequency"] = "annual"
        
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
        
        # Add state to policy content
        policy_content["state"] = request.state
        
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
                "Content-Disposition": f"attachment; filename=ai_policy_{request.company_name.replace(' ', '_')}_{request.industry}_{request.state}_enhanced.pdf"
            }
        )
        
    except Exception as e:
        print(f"Error generating enhanced policy: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/compliance-requirements")
async def get_compliance_requirements(industry: str, state: str):
    """Get AI compliance requirements for preview"""
    try:
        # Get industry template
        industry_template = INDUSTRY_TEMPLATES.get(
            industry.lower(),
            get_default_template()
        )
        
        # Get state requirements
        state_reqs = STATE_COMPLIANCE.get(
            state.upper(),
            get_default_state_compliance()
        )
        
        # Create policy generator to get compliance config
        policy_generator = PolicyGenerator()
        compliance_config = policy_generator.compliance_config
        
        # Get detailed compliance requirements
        industry_compliance = compliance_config.get_industry_compliance(industry)
        state_requirements = compliance_config.get_state_requirements(state)
        
        return ComplianceRequirementsResponse(
            industry_compliance=industry_compliance,
            state_requirements=state_requirements,
            ai_transparency_level=industry_template.get("ai_compliance", {}).get("transparency_level", "medium"),
            bias_testing_frequency=industry_template.get("ai_compliance", {}).get("bias_testing_frequency", "quarterly"),
            audit_retention_period=state_requirements.get("retention_period", "3 years")
        )
        
    except Exception as e:
        print(f"Error getting compliance requirements: {str(e)}")
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
            "audit_frequency",
            "ai_compliance"
        ]
    )

@router.get("/templates")
async def list_industry_templates():
    """List all available industry templates"""
    return {
        "industries": list(INDUSTRY_TEMPLATES.keys()),
        "templates": INDUSTRY_TEMPLATES,
        "state_compliance": STATE_COMPLIANCE,
        "customization_options": {
            "compliance_priority": ["balanced", "strict", "flexible"],
            "risk_tolerance": ["low", "medium", "high"],
            "data_sensitivity": ["low", "medium", "high", "critical"],
            "ai_transparency_level": ["low", "medium", "high", "critical"],
            "bias_testing_frequency": ["weekly", "monthly", "quarterly", "semi-annual", "annual"]
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
        
        # Check if AI compliance sections are included
        ai_compliance_sections = [
            "AI Transparency Requirements",
            "AI Bias Prevention Measures",
            "AI Audit Trail Requirements"
        ]
        
        return {
            "industry": request.industry,
            "state": request.state,
            "sections": sections_preview,
            "compliance_frameworks": industry_template.get("compliance_frameworks", []),
            "ai_compliance_included": any(section in sections_preview for section in ai_compliance_sections),
            "estimated_pages": estimate_page_count(sections_preview, request.ai_tools)
        }
        
    except Exception as e:
        print(f"Error previewing sections: {str(e)}")
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/states")
async def list_states_with_compliance():
    """List all states with their AI compliance requirements"""
    return {
        "states": list(STATE_COMPLIANCE.keys()),
        "compliance_details": STATE_COMPLIANCE,
        "default_states": ["CA", "NY", "IL", "CO", "WA"],
        "states_with_ai_laws": {
            "CA": "SB 1001 (Bot Disclosure), CCPA",
            "NY": "NYC Local Law 144 (Bias Audits)",
            "IL": "BIPA (Biometric Data)",
            "CO": "CPA (AI Assessments)",
            "WA": "Algorithmic Accountability"
        }
    }

@router.get("/test")
async def test_endpoint():
    """Test endpoint to verify API is working"""
    return {
        "message": "Policies API is working",
        "available_industries": list(INDUSTRY_TEMPLATES.keys()),
        "ai_compliance_enabled": True,
        "version": "1.11a"
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
        "audit_frequency": "semi-annual",
        "ai_compliance": {
            "transparency_level": "medium",
            "bias_testing_frequency": "quarterly",
            "audit_retention": "3 years"
        }
    }

def get_default_state_compliance() -> Dict:
    """Return default state compliance for states without specific requirements"""
    return {
        "frameworks": ["General State Privacy Laws"],
        "ai_specific": ["General AI disclosure requirements"],
        "retention_period": "3 years"
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
    
    # Add extra pages for AI compliance sections
    ai_compliance_sections = [
        "AI Transparency Requirements",
        "AI Bias Prevention Measures",
        "AI Audit Trail Requirements"
    ]
    ai_compliance_pages = sum(3 for section in sections if section in ai_compliance_sections)
    
    return int(base_pages + section_pages + tool_pages + ai_compliance_pages)