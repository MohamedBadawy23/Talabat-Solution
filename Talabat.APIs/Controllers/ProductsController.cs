using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.APIs.Helpers;
using Talabat.Core.Entities;
using Talabat.Core.Repositories.Contract;
using Talabat.Core.Specifications;
using Talabat.Core.Specifications.Products_Specs;

namespace Talabat.APIs.Controllers
{
   
    public class ProductsController : BaseApiController
    {
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<ProductBrand> _brandRepo;
        private readonly IGenericRepository<ProductType> _categoryRepo;
        private readonly IMapper _mapper;

        public ProductsController(
            IGenericRepository<Product> productRepo,
            IGenericRepository<ProductBrand> brandRepo,
            IGenericRepository<ProductType> categoryRepo,
            IMapper mapper
            )
        {
            _productRepo = productRepo;
            _brandRepo = brandRepo;
            _categoryRepo = categoryRepo;
            _mapper = mapper;
        }

        // /api/Products?sort=PriceAsc
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ProductToReturnDto>>> GetProducts([FromQuery] ProductSpecParams productSpec)
        {
            //var products = await _productRepo.GetAllAsync();

            ////var spec = new BaseSpecifications<Product>();
            var spec = new ProductWithBrandAndCategorySpecifications(productSpec);
            var products = await _productRepo.GetAllWithSpecAsync(spec);

            var result = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

            var countSpec = new ProductWithFilterationCountSpecifications(productSpec);
            int count = await _productRepo.GetCountAsync(countSpec);

            return Ok(new Pagination<ProductToReturnDto>(productSpec.PageSize,productSpec.PageIndex,count,result));
        }


        [ProducesResponseType(typeof(ProductToReturnDto),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse),StatusCodes.Status404NotFound)]
        [HttpGet("{id}")] // GET : /api/Products/3
        public async Task<ActionResult<ProductToReturnDto>> GetProductById(int id)
        {
            //var product = await _productRepo.GetAsync(id);
            var specs = new ProductWithBrandAndCategorySpecifications(id);
            var product = await _productRepo.GetWithSpecAsync(specs);
            if (product is null)
                return NotFound(new ApiResponse(404)); // 404


            var result = _mapper.Map<Product, ProductToReturnDto>(product);
            return Ok(result); // 200
        }


        [HttpGet("brands")] // GET : /api/Products/brands
        public async Task<ActionResult<IReadOnlyList<ProductBrand>>> GetBrands()
        {
           var brands = await _brandRepo.GetAllAsync();

            return Ok(brands);
        }

        [HttpGet("types")] // GET : /api/Products/categories
        public async Task<ActionResult<IReadOnlyList<ProductType>>> GetCategories()
        {
            var categories = await  _categoryRepo.GetAllAsync();

            return Ok(categories);
        }


    }
}
