from fastapi.responses import RedirectResponse
from stripe_config import DOMAIN
import stripe  # Make sure this is imported at the top if not already

@app.post("/create-checkout-session")
async def create_checkout_session():
    try:
        checkout_session = stripe.checkout.Session.create(
            payment_method_types=["card"],
            line_items=[
                {
                    "price_data": {
                        "currency": "usd",
                        "product_data": {
                            "name": "AI Policy Document",
                        },
                        "unit_amount": 1000,  # $10.00 USD
                    },
                    "quantity": 1,
                }
            ],
            mode="payment",
            success_url=f"{DOMAIN}/success",
            cancel_url=f"{DOMAIN}/cancel",
        )
        return {"id": checkout_session.id}
    except Exception as e:
        return {"error": str(e)}
