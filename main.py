"""
Root-level main.py for Render deployment
The actual application is in app/main.py
"""
from app.main import app

# Export the FastAPI app instance for Uvicorn
__all__ = ["app"]
