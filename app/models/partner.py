# compligenie-backend/app/models/partner.py

from pydantic import BaseModel, EmailStr
from typing import Optional, Dict, List
from datetime import datetime
from enum import Enum

class PartnerStatus(str, Enum):
    PENDING = "pending"
    ACTIVE = "active"
    SUSPENDED = "suspended"

class PartnerTier(str, Enum):
    STARTER = "starter"     # 0-50 policies/month, 20% revenue share
    GROWTH = "growth"       # 51-200 policies/month, 30% revenue share
    ENTERPRISE = "enterprise"  # 200+ policies/month, 40% revenue share

class Partner(BaseModel):
    id: str
    company_name: str
    email: EmailStr
    contact_name: str
    partner_type: str  # legal, hr, insurance, consulting
    
    # Stripe Connect fields
    stripe_account_id: Optional[str] = None
    stripe_onboarding_completed: bool = False
    
    # Usage tracking
    policies_generated_total: int = 0
    policies_generated_monthly: int = 0
    last_policy_date: Optional[datetime] = None
    
    # Revenue tracking
    total_revenue: float = 0.0
    monthly_revenue: float = 0.0
    pending_payout: float = 0.0
    last_payout_date: Optional[datetime] = None
    
    # Status and tier
    status: PartnerStatus = PartnerStatus.PENDING
    tier: PartnerTier = PartnerTier.STARTER
    revenue_share_percentage: int = 20
    
    # API access
    api_key: str
    api_secret: str
    
    # Branding
    branding: Optional[Dict] = None
    
    created_at: datetime
    updated_at: Optional[datetime] = None

class PolicyGeneration(BaseModel):
    id: str
    partner_id: str
    policy_id: str
    company_name: str
    industry: str
    employee_count: int
    base_price: float = 10.0  # $10 per policy
    partner_revenue: float  # Calculated based on revenue share
    stripe_payment_intent_id: Optional[str] = None
    status: str = "pending"  # pending, completed, failed
    created_at: datetime

class PartnerEarnings(BaseModel):
    partner_id: str
    period: str  # "2024-01" for monthly
    policies_count: int
    gross_revenue: float
    partner_share: float
    status: str = "pending"  # pending, paid, processing
    stripe_transfer_id: Optional[str] = None
    created_at: datetime