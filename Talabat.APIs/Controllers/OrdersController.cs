using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.Core;
using Talabat.Core.Entities.Order;
using Talabat.Core.Services.Contract;

namespace Talabat.APIs.Controllers
{
  
    public class OrdersController : BaseApiController
    {
        private readonly IOrderService _orderService;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public OrdersController(IOrderService orderService, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _orderService = orderService;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }


        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [HttpPost] // POST : /api/Orders
        [Authorize]
        public async Task<ActionResult<OrderToReturnDto>> CreateOrder(OrderDto model)
        {
            var buyerEmail =  User.FindFirstValue(ClaimTypes.Email);
            var address =  _mapper.Map<AddressDto, Address>(model.shipToAddress);
            var order = await _orderService.CreateOrderAsync(buyerEmail, model.BasketId, model.DeliveryMethodId, address);
            
            if (order is null) return BadRequest(new ApiResponse(400, "There is a Problem With Your Order!"));

            var result =  _mapper.Map<Order, OrderToReturnDto>(order);

            return Ok(result);

        }


        [ProducesResponseType(typeof(IReadOnlyList<OrderToReturnDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet] // GET : /api/Orders
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<OrderToReturnDto>>> GetOrdersForUser()
        {
            var buyerEmail =  User.FindFirstValue(ClaimTypes.Email);

            var orders = await _orderService.GetOrdersForSpecificUserAsync(buyerEmail);
            if (orders is null) return NotFound(new ApiResponse(404, "There is no Order For U"));

            

            return Ok(_mapper.Map< IReadOnlyList <OrderToReturnDto>>(orders));
        }



        [ProducesResponseType(typeof(OrderToReturnDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [HttpGet("{id}")] // GET : /api/Orders/1
        [Authorize]
        public async Task<ActionResult<OrderToReturnDto>> GetOrderByIdForUser(int id)
        {
            var buyerEmail =  User.FindFirstValue(ClaimTypes.Email);
            var order = await _orderService.GetOrderByIdForSpecificUserAsync(buyerEmail, id);
            if (order is null) return NotFound(new ApiResponse(404, $"There is not Order With id :{id} For U"));

            return Ok(_mapper.Map<OrderToReturnDto>(order));
        }

        [HttpGet("DeliveryMethods")] // GET : /api/Orders/DeliveryMethods
        public async Task<ActionResult<IReadOnlyList<DeliveryMethod>>> GetDeliveryMethods()
        {
           var deliveryMethods = await  _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();

            return Ok(deliveryMethods);
        }
        

    }
}
