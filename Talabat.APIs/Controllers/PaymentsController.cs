using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Talabat.APIs.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Services.Contract;

namespace Talabat.APIs.Controllers
{

    [Authorize]
    public class PaymentsController : BaseApiController
    {
        private readonly IPaymentService _paymentService;
        const string endpointSecret = "whsec_83ddf1afc3d3fe4215cddfd6cd71c916be1e66ac0e85aa0f3f1b113ffe01b9d8";

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [ProducesResponseType(typeof(CustomerBasket), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost("{basketId}")] // POST : /api/Payments
        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntentId(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);
            if (basket is null) return BadRequest(new ApiResponse(400, "There is a problem with your basket"));
            return Ok(basket);
        }

        [AllowAnonymous]
        [HttpPost("webhook")] // POST : /api/Payments/webhook
        // https://localhost:7271/api/Payments/webhook
        public async Task<IActionResult> StripeWebHook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], endpointSecret);

                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                // Handle the event
                if (stripeEvent.Type == Events.PaymentIntentPaymentFailed)
                {
                   await _paymentService.UpdatePaymentIntentToSuccssedOrFalid(paymentIntent.Id, false);
                }
                else if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    await _paymentService.UpdatePaymentIntentToSuccssedOrFalid(paymentIntent.Id, true);
                }
                // ... handle other event types
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }

                return Ok();
            }
            catch (StripeException e)
            {
                return BadRequest();
            }
        }
    }


}

