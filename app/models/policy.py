# app/models/policy.py

from pydantic import BaseModel
from typing import List, Optional, Dict

class PolicySubsection(BaseModel):
    title: str
    content: str

class PolicySection(BaseModel):
    title: str
    content: str
    subsections: Optional[List[PolicySubsection]] = []

class PolicyDocument(BaseModel):
    title: str
    company_name: str
    effective_date: str
    industry: str
    state: str
    ai_tools: List[str]
    employee_count: int
    sections: List[PolicySection]
    partner_id: Optional[str] = None

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