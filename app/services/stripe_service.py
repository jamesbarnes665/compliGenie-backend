# compligenie-backend/app/services/stripe_service.py

import stripe
import os
from typing import Dict, Optional
from datetime import datetime
import logging
from dotenv import load_dotenv

# Ensure environment variables are loaded
load_dotenv()

logger = logging.getLogger(__name__)

# Set Stripe API key at module level
STRIPE_SECRET_KEY = os.getenv("STRIPE_SECRET_KEY")
if STRIPE_SECRET_KEY:
    stripe.api_key = STRIPE_SECRET_KEY
    logger.info(f"Stripe API key set at module level: {STRIPE_SECRET_KEY[:20]}...")
else:
    logger.error("No STRIPE_SECRET_KEY found in environment!")

class StripeConnectService:
    def __init__(self):
        self.api_key = STRIPE_SECRET_KEY
        self.webhook_secret = os.getenv("STRIPE_WEBHOOK_SECRET")
        
        # Debug logging
        logger.info(f"Initializing StripeConnectService")
        logger.info(f"STRIPE_SECRET_KEY present: {bool(self.api_key)}")
        logger.info(f"stripe.api_key is set: {bool(stripe.api_key)}")
    
    def create_connect_account(self, partner_data: Dict) -> Dict:
        """Create a Stripe Connect Express account"""
        try:
            logger.info(f"Creating Stripe Connect account for: {partner_data.get('email')}")
            logger.info(f"Stripe API key is: {stripe.api_key[:20] if stripe.api_key else 'NOT SET'}...")
            
            account = stripe.Account.create(
                type="express",
                country="US",
                email=partner_data["email"],
                capabilities={
                    "transfers": {"requested": True},
                },
                business_profile={
                    "name": partner_data["business_name"],
                    "url": partner_data.get("website", ""),
                },
            )
            return {
                "success": True,
                "account_id": account.id,
                "details_submitted": account.details_submitted,
            }
        except stripe.error.StripeError as e:
            logger.error(f"Stripe error creating account: {str(e)}")
            return {"success": False, "error": str(e)}
    
    def create_account_link(self, account_id: str, return_url: str, refresh_url: str) -> Dict:
        """Create an account link for Connect onboarding"""
        try:
            account_link = stripe.AccountLink.create(
                account=account_id,
                return_url=return_url,
                refresh_url=refresh_url,
                type="account_onboarding",
            )
            return {"success": True, "url": account_link.url}
        except stripe.error.StripeError as e:
            logger.error(f"Stripe error creating account link: {str(e)}")
            return {"success": False, "error": str(e)}
    
    def retrieve_account(self, account_id: str) -> Dict:
        """Retrieve Connect account details"""
        try:
            account = stripe.Account.retrieve(account_id)
            return {
                "success": True,
                "charges_enabled": account.charges_enabled,
                "payouts_enabled": account.payouts_enabled,
                "details_submitted": account.details_submitted,
            }
        except stripe.error.StripeError as e:
            logger.error(f"Stripe error retrieving account: {str(e)}")
            return {"success": False, "error": str(e)}
    
    def create_payout(self, account_id: str, amount: float, description: str = "Partner payout") -> Dict:
        """Create a payout for a Connect account"""
        try:
            # Create a transfer to the Connect account
            transfer = stripe.Transfer.create(
                amount=int(amount * 100),  # Convert to cents
                currency="usd",
                destination=account_id,
                description=description,
            )
            return {"success": True, "transfer_id": transfer.id, "amount": amount}
        except stripe.error.StripeError as e:
            logger.error(f"Stripe error creating transfer: {str(e)}")
            return {"success": False, "error": str(e)}
    
    def process_webhook(self, payload: bytes, sig_header: str) -> Dict:
        """Process Stripe webhooks"""
        try:
            event = stripe.Webhook.construct_event(
                payload, sig_header, self.webhook_secret
            )
            
            # Handle different event types
            if event["type"] == "account.updated":
                account = event["data"]["object"]
                return {
                    "success": True,
                    "type": "account.updated",
                    "account_id": account["id"],
                    "charges_enabled": account.get("charges_enabled", False),
                    "payouts_enabled": account.get("payouts_enabled", False),
                }
            
            return {"success": True, "type": event["type"]}
            
        except ValueError as e:
            logger.error(f"Invalid payload: {str(e)}")
            return {"success": False, "error": "Invalid payload"}
        except stripe.error.SignatureVerificationError as e:
            logger.error(f"Invalid signature: {str(e)}")
            return {"success": False, "error": "Invalid signature"}