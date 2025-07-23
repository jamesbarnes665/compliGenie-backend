# compligenie-backend/app/api/partners.py

from fastapi import APIRouter, HTTPException, Header, Depends, Request
from fastapi.responses import JSONResponse
from typing import Optional, Dict, List
from datetime import datetime, timedelta
import secrets
import json
import os
from app.models.partner import Partner, PolicyGeneration, PartnerEarnings, PartnerStatus, PartnerTier
from app.services.stripe_service import StripeConnectService
from app.services.partner_store import partner_store

router = APIRouter(prefix="/api/partners", tags=["partners"])
stripe_service = StripeConnectService()

# Dependency to get current partner from API key
async def get_current_partner(x_api_key: str = Header(...)) -> Partner:
    partner = partner_store.get_by_api_key(x_api_key)
    if not partner:
        raise HTTPException(status_code=401, detail="Invalid API key")
    return partner

@router.post("/register")
async def register_partner(partner_data: Dict):
    """Register a new partner and create Stripe Connect account"""
    try:
        # Generate API credentials
        api_key = f"pk_{partner_data['partner_type'][:3]}_{secrets.token_urlsafe(16)}"
        api_secret = f"sk_{secrets.token_urlsafe(32)}"
        
        # Create partner object
        partner = Partner(
            id=f"partner_{secrets.token_urlsafe(8)}",
            company_name=partner_data["company_name"],
            email=partner_data["email"],
            contact_name=partner_data["contact_name"],
            partner_type=partner_data["partner_type"],
            api_key=api_key,
            api_secret=api_secret,
            created_at=datetime.utcnow()
        )
        
        # Create Stripe Connect account - map company_name to business_name
        stripe_data = {
            "business_name": partner_data["company_name"],
            "email": partner_data["email"],
            "website": partner_data.get("website", "")
        }
        stripe_account = stripe_service.create_connect_account(stripe_data)
        
        if stripe_account["success"]:
            partner.stripe_account_id = stripe_account["account_id"]
        else:
            raise HTTPException(status_code=400, detail=f"Failed to create Stripe account: {stripe_account.get('error', 'Unknown error')}")
        
        # Store partner
        partner_store.create(partner)
        
        # Return registration details (without onboarding URL)
        return {
            "success": True,
            "partner_id": partner.id,
            "api_key": partner.api_key,
            "api_secret": partner.api_secret,
            "stripe_account_id": partner.stripe_account_id,
            "message": "Partner registered successfully. Use the /stripe-onboarding endpoint to get the onboarding link."
        }
        
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))

@router.post("/stripe-onboarding")
async def create_stripe_onboarding(partner: Partner = Depends(get_current_partner)):
    """Create Stripe Connect onboarding link"""
    if not partner.stripe_account_id:
        raise HTTPException(status_code=400, detail="No Stripe account found")
    
    frontend_url = os.getenv("FRONTEND_URL", "http://localhost:3000")
    return_url = f"{frontend_url}/partners/dashboard"
    refresh_url = f"{frontend_url}/partners/stripe-onboarding"
    
    result = stripe_service.create_account_link(
        account_id=partner.stripe_account_id,
        return_url=return_url,
        refresh_url=refresh_url
    )
    
    if result["success"]:
        return {"url": result["url"]}
    else:
        raise HTTPException(status_code=400, detail=result.get("error", "Failed to create onboarding link"))

@router.get("/me")
async def get_partner_details(partner: Partner = Depends(get_current_partner)):
    """Get current partner details"""
    # Retrieve latest Stripe account status
    if partner.stripe_account_id:
        stripe_status = stripe_service.retrieve_account(partner.stripe_account_id)
        if stripe_status["success"]:
            partner.stripe_onboarding_completed = (
                stripe_status.get("charges_enabled", False) and 
                stripe_status.get("payouts_enabled", False)
            )
            # Update partner status
            if partner.stripe_onboarding_completed and partner.status == PartnerStatus.PENDING:
                partner.status = PartnerStatus.ACTIVE
            partner_store.update(partner.id, partner.dict())
    
    return {
        "partner_id": partner.id,
        "business_name": partner.company_name,
        "email": partner.email,
        "status": partner.status,
        "tier": partner.tier,
        "stripe_connected": partner.stripe_onboarding_completed,
        "stripe_account_id": partner.stripe_account_id,
        "total_policies": partner.policies_generated_total,
        "total_revenue": partner.total_revenue,
        "available_balance": partner.pending_payout
    }

@router.get("/earnings")
async def get_partner_earnings(partner: Partner = Depends(get_current_partner)):
    """Get partner earnings summary"""
    return {
        "total_revenue": partner.total_revenue,
        "available_balance": partner.pending_payout,
        "pending_payouts": 0,
        "last_payout_date": partner.last_payout_date,
        "current_tier": partner.tier,
        "revenue_share_percentage": partner.revenue_share_percentage,
        "policies_this_month": partner.policies_generated_monthly,
        "total_policies": partner.policies_generated_total
    }

@router.post("/payout")
async def request_payout(
    payout_data: Dict,
    partner: Partner = Depends(get_current_partner)
):
    """Request a payout of available balance"""
    amount = payout_data.get("amount", 0)
    
    if amount <= 0:
        raise HTTPException(status_code=400, detail="Invalid payout amount")
    
    if amount > partner.pending_payout:
        raise HTTPException(status_code=400, detail="Insufficient balance")
    
    if not partner.stripe_onboarding_completed:
        raise HTTPException(status_code=400, detail="Complete Stripe onboarding first")
    
    # Create Stripe transfer
    result = stripe_service.create_payout(
        account_id=partner.stripe_account_id,
        amount=amount,
        description=f"Partner payout - {partner.company_name}"
    )
    
    if result["success"]:
        # Update partner balance
        partner.pending_payout -= amount
        partner.last_payout_date = datetime.utcnow()
        partner_store.update(partner.id, partner.dict())
        
        return {
            "success": True,
            "message": f"Payout of ${amount} initiated",
            "transfer_id": result["transfer_id"],
            "remaining_balance": partner.pending_payout
        }
    else:
        raise HTTPException(status_code=400, detail=result.get("error", "Payout failed"))

@router.post("/webhooks/stripe")
async def handle_stripe_webhook(request: Request):
    """Handle Stripe webhooks"""
    payload = await request.body()
    sig_header = request.headers.get("stripe-signature")
    
    if not sig_header:
        raise HTTPException(status_code=400, detail="No signature header")
    
    result = stripe_service.process_webhook(payload, sig_header)
    
    if not result["success"]:
        raise HTTPException(status_code=400, detail=result.get("error", "Webhook processing failed"))
    
    # Handle specific webhook events
    if result["type"] == "account.updated":
        # Update partner status based on Stripe account status
        account_id = result["account_id"]
        partner = partner_store.get_by_stripe_account(account_id)
        if partner:
            partner.stripe_onboarding_completed = (
                result.get("charges_enabled", False) and 
                result.get("payouts_enabled", False)
            )
            if partner.stripe_onboarding_completed and partner.status == PartnerStatus.PENDING:
                partner.status = PartnerStatus.ACTIVE
            partner_store.update(partner.id, partner.dict())
    
    return {"received": True}



@router.post("/test-charge")
async def create_test_charge():
    """Create a test charge to fund the platform account (TEST MODE ONLY)"""
    try:
        import stripe
        
        # Create a test charge using the special test card
        charge = stripe.Charge.create(
            amount=10000,  # $100.00 in cents
            currency="usd",
            source="tok_bypassPending",  # Special test token
            description="Test funding for platform account"
        )
        
        return {
            "success": True,
            "charge_id": charge.id,
            "amount": charge.amount / 100,
            "message": "Platform account funded with test money"
        }
    except stripe.error.StripeError as e:
        raise HTTPException(status_code=400, detail=str(e))

@router.put("/branding")
async def update_partner_branding(
    branding_data: Dict,
    partner: Partner = Depends(get_current_partner)
):
    """Update partner branding settings"""
    try:
        # Create branding object
        from app.models.partner import PartnerBranding
        
        branding = PartnerBranding(
            logo_url=branding_data.get("logo_url"),
            company_name=branding_data.get("company_name", partner.company_name),
            primary_color=branding_data.get("primary_color", "#1a365d"),
            secondary_color=branding_data.get("secondary_color", "#2563eb"),
            contact_phone=branding_data.get("contact_phone"),
            contact_email=branding_data.get("contact_email"),
            contact_website=branding_data.get("contact_website"),
            powered_by_text=branding_data.get("powered_by_text", f"Powered by {partner.company_name}"),
            show_compligenie_branding=branding_data.get("show_compligenie_branding", True),
            footer_text=branding_data.get("footer_text")
        )
        
        # Update partner branding
        partner.branding = branding
        partner.updated_at = datetime.utcnow()
        partner_store.update(partner.id, partner.dict())
        
        return {
            "success": True,
            "message": "Branding updated successfully",
            "branding": branding.dict()
        }
        
    except Exception as e:
        raise HTTPException(status_code=400, detail=str(e))

@router.get("/branding")
async def get_partner_branding(partner: Partner = Depends(get_current_partner)):
    """Get current partner branding settings"""
    if partner.branding:
        return partner.branding.dict()
    else:
        # Return default branding
        return {
            "company_name": partner.company_name,
            "primary_color": "#1a365d",
            "secondary_color": "#2563eb",
            "powered_by_text": f"Powered by {partner.company_name}",
            "show_compligenie_branding": True
        }

@router.post("/branding/logo")
async def upload_partner_logo(
    partner: Partner = Depends(get_current_partner)
):
    """Upload partner logo (returns URL for now - implement S3 later)"""
    # For MVP, we'll use a placeholder
    # In production, this would upload to S3/Cloudinary
    
    logo_url = f"https://ui-avatars.com/api/?name={partner.company_name}&size=200&background=random"
    
    return {
        "success": True,
        "logo_url": logo_url,
        "message": "Logo uploaded successfully (using placeholder for MVP)"
    }
