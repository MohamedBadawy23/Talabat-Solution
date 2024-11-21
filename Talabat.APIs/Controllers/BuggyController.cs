using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Talabat.APIs.Errors;
using Talabat.Respository.Data;

namespace Talabat.APIs.Controllers
{
   
    public class BuggyController : BaseApiController
    {
        private readonly StoreDbContext _context;

        public BuggyController(StoreDbContext context)
        {
            _context = context;
        }

        [HttpGet("notfound")] // GET: /api/buggy/notfound
        public ActionResult GetNotFoundRequest()
        {
            var brand = _context.Brands.Find(100);
            if (brand is null)
                return NotFound(new ApiResponse(404));
            return Ok(brand);
        }


        [HttpGet("servererror")] // GET : /api/buggy/servererror
        public ActionResult GetServerError()
        {
            var brand = _context.Brands.Find(100);
            var brandToReturn  = brand.ToString(); // Will Throw Exception (NullReferenceException)

            return Ok(brandToReturn);
            
        }


        [HttpGet("badrequest")] // GET : /api/buggy/badrequest
        public ActionResult GetBadRequest()
        {
            return BadRequest(new ApiResponse(400));
        }

        [HttpGet("badrequest/{id}")] // GET : /api/buggy/badrequest/5
        public ActionResult getBadRequest(int id) // Validation Error
        {
            return Ok();
        }


        [HttpGet("unauthorized")] // GET : /api/buggy/unauthorized
        public ActionResult GetUnauthorizedError()
        {
            return Unauthorized(new ApiResponse(401));
        }


    }
}
