# compligenie-backend/app/models/partner.py

from pydantic import BaseModel, EmailStr, Field
from typing import Optional, Dict, List
from datetime import datetime
from enum import Enum

class PartnerStatus(str, Enum):
    PENDING = "pending"
    ACTIVE = "active"
    SUSPENDED = "suspended"

class PartnerTier(str, Enum):
    STARTER = "starter"
    GROWTH = "growth"
    ENTERPRISE = "enterprise"

class PartnerBranding(BaseModel):
    """Partner branding configuration"""
    logo_url: Optional[str] = None
    company_name: str
    primary_color: str = "#1a365d"  # Default blue
    secondary_color: str = "#2563eb"
    contact_phone: Optional[str] = None
    contact_email: Optional[str] = None
    contact_website: Optional[str] = None
    powered_by_text: Optional[str] = None
    show_compligenie_branding: bool = True
    footer_text: Optional[str] = None

class Partner(BaseModel):
    id: str
    company_name: str
    email: EmailStr
    contact_name: str
    partner_type: str
    
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
    api_secret: Optional[str] = None
    
    # Branding - NEW!
    branding: Optional[PartnerBranding] = None
    
    # Timestamps
    created_at: datetime = Field(default_factory=datetime.utcnow)
    updated_at: Optional[datetime] = None

class PolicyGeneration(BaseModel):
    id: str
    partner_id: str
    policy_id: str
    company_name: str
    industry: str
    employee_count: int
    base_price: float = 10.0
    partner_revenue: float
    stripe_payment_intent_id: Optional[str] = None
    status: str = "pending"
    created_at: datetime = Field(default_factory=datetime.utcnow)

class PartnerEarnings(BaseModel):
    partner_id: str
    period: str
    policies_count: int
    gross_revenue: float
    partner_share: float
    status: str = "pending"
    stripe_transfer_id: Optional[str] = None
    created_at: datetime = Field(default_factory=datetime.utcnow)