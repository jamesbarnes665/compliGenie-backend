# compligenie-backend/app/services/partner_store.py

from typing import Dict, Optional, List
from app.models.partner import Partner
import json
import os
from datetime import datetime

class PartnerStore:
    """Simple in-memory store for MVP - replace with database later"""
    
    def __init__(self):
        self.partners: Dict[str, Partner] = {}
        self.api_key_map: Dict[str, str] = {}  # api_key -> partner_id
        self._load_from_file()
    
    def create(self, partner: Partner) -> Partner:
        """Create new partner"""
        self.partners[partner.id] = partner
        self.api_key_map[partner.api_key] = partner.id
        self._save_to_file()
        return partner
    
    def get_by_id(self, partner_id: str) -> Optional[Partner]:
        """Get partner by ID"""
        return self.partners.get(partner_id)
    
    def get_by_api_key(self, api_key: str) -> Optional[Partner]:
        """Get partner by API key"""
        partner_id = self.api_key_map.get(api_key)
        if partner_id:
            return self.partners.get(partner_id)
        return None
    
    def get_by_email(self, email: str) -> Optional[Partner]:
        """Get partner by email"""
        for partner in self.partners.values():
            if partner.email == email:
                return partner
        return None
    
    def update(self, partner_id: str, updates: Dict) -> Optional[Partner]:
        """Update partner"""
        if partner_id in self.partners:
            partner_dict = self.partners[partner_id].dict()
            partner_dict.update(updates)
            partner_dict["updated_at"] = datetime.utcnow()
            self.partners[partner_id] = Partner(**partner_dict)
            self._save_to_file()
            return self.partners[partner_id]
        return None
    
    def list_all(self) -> List[Partner]:
        """List all partners"""
        return list(self.partners.values())
    
    def reset_monthly_counters(self):
        """Reset monthly counters - call this via cron job"""
        for partner in self.partners.values():
            partner.policies_generated_monthly = 0
            partner.monthly_revenue = 0.0
        self._save_to_file()
    
    def _save_to_file(self):
        """Persist to file for MVP - replace with database later"""
        data = {
            "partners": {k: v.dict() for k, v in self.partners.items()},
            "api_key_map": self.api_key_map
        }
        
        os.makedirs("data", exist_ok=True)
        with open("data/partners.json", "w") as f:
            json.dump(data, f, indent=2, default=str)
    
    def _load_from_file(self):
        """Load from file on startup"""
        if os.path.exists("data/partners.json"):
            try:
                with open("data/partners.json", "r") as f:
                    data = json.load(f)
                    
                for partner_id, partner_data in data.get("partners", {}).items():
                    # Convert string dates back to datetime
                    if partner_data.get("created_at"):
                        partner_data["created_at"] = datetime.fromisoformat(partner_data["created_at"])
                    if partner_data.get("updated_at"):
                        partner_data["updated_at"] = datetime.fromisoformat(partner_data["updated_at"])
                    if partner_data.get("last_policy_date"):
                        partner_data["last_policy_date"] = datetime.fromisoformat(partner_data["last_policy_date"])
                    if partner_data.get("last_payout_date"):
                        partner_data["last_payout_date"] = datetime.fromisoformat(partner_data["last_payout_date"])
                    
                    self.partners[partner_id] = Partner(**partner_data)
                    
                self.api_key_map = data.get("api_key_map", {})
            except Exception as e:
                print(f"Error loading partners: {e}")

# Global instance
partner_store = PartnerStore()