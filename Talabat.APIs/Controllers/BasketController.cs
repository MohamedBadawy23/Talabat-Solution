using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;

namespace Talabat.APIs.Controllers
{

    public class BasketController : BaseApiController
    {
        private readonly IBasketRepository _basketRepository;
        private readonly IMapper _mapper;

        public BasketController(IBasketRepository basketRepository, IMapper mapper)
        {
            _basketRepository = basketRepository;
            _mapper = mapper;
        }

        [HttpGet] // GET : /api/basket
        public async Task<ActionResult<CustomerBasket>> GetBasket(string id)
        {
            var basket = await _basketRepository.GetBasketAsync(id);
            if (basket is null) new CustomerBasket() { Id = id };

            return Ok(basket);
        }

        [HttpPost] // POST : /api/basket
        public async Task<ActionResult<CustomerBasket>> CreatedOrUpdatedBasket(CustomerBasketDto model)
        {
            var mappedBasket =  _mapper.Map<CustomerBasket>(model);
            var basket = await _basketRepository.UpdateBasketAsync(mappedBasket);
            if (basket is null) return BadRequest(new ApiResponse(400));
            return Ok(basket);
        }


        [HttpDelete] // DELETE : /api/basket
        public async Task DeleteBasket(string id)
        {
             await _basketRepository.DeleteBasketAsync(id);
        }



    }
}
