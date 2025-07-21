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
    sections: List[PolicySection]
    company_name: str
    industry: str
    ai_tools: List[str]
    employee_count: int
    partner_id: Optional[str] = None