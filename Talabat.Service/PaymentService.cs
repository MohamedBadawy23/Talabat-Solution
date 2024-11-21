using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core;
using Talabat.Core.Entities;
using Talabat.Core.Entities.Order;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Services.Contract;
using Talabat.Core.Specifications.Order_Specs;
using Product = Talabat.Core.Entities.Product;

namespace Talabat.Service
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IBasketRepository _basketRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IConfiguration configuration, IBasketRepository basketRepository,IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _basketRepository = basketRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<CustomerBasket?> CreateOrUpdatePaymentIntent(string basketId)
        {
            // Secret Key
            StripeConfiguration.ApiKey = _configuration["StripeKeys:Secretkey"];

            // Get Basket
            var basket = await _basketRepository.GetBasketAsync(basketId);
            if (basket is null) return null;
            var ShippingPrice = 0m;
            if (basket.DeliveryMethodId.HasValue)
            {
               var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetAsync(basket.DeliveryMethodId.Value);
                ShippingPrice = deliveryMethod.Cost;   
            }

            if(basket.Items.Count > 0)
            {
                foreach (var item in basket.Items)
                {
                   var product = await _unitOfWork.Repository<Product>().GetAsync(item.Id);
                    if(item.Price != product.Price)
                    {
                        item.Price = product.Price;
                    }
                }
            }

            // SubTotal
             var subTotal =  basket.Items.Sum(I => I.Price * I.Quantity);

            var service = new PaymentIntentService();
            PaymentIntent paymentIntent;
            if (string.IsNullOrEmpty(basket.PaymentIntentId))
            {
                // Create
                var Options = new PaymentIntentCreateOptions()
                {
                    Amount = (long)(subTotal * 100 + ShippingPrice * 100),
                    Currency = "usd",
                    PaymentMethodTypes = new List<string>() { "card" },
                };
                paymentIntent = await service.CreateAsync(Options);
                basket.PaymentIntentId = paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;
            }
            else
            {
                var Options = new PaymentIntentUpdateOptions()
                {
                    Amount = (long)(subTotal * 100 + ShippingPrice * 100)
                };

                // Update
               paymentIntent = await service.UpdateAsync(basket.PaymentIntentId, Options);
                basket.PaymentIntentId = paymentIntent.Id;
                basket.ClientSecret = paymentIntent.ClientSecret;
            }

           await _basketRepository.UpdateBasketAsync(basket);
            
            return basket;
            
        }

        public async Task<Order> UpdatePaymentIntentToSuccssedOrFalid(string paymentIntetId, bool flag)
        {
            var spec = new OrderWithPaymentIntentSpecifications(paymentIntetId);
           var order = await _unitOfWork.Repository<Order>().GetWithSpecAsync(spec);
            if (flag)
            {
                order.Status = OrderStatus.PaymentReceived;
            }
            else
            {
                order.Status = OrderStatus.PaymentFailed;
            }

            _unitOfWork.Repository<Order>().Update(order);
            await _unitOfWork.CompleteAsync();
            return order;
        } 


    }
}
