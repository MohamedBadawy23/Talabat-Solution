using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Talabat.APIs.Dtos;
using Talabat.APIs.Errors;
using Talabat.APIs.Extensions;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services.Contract;

namespace Talabat.APIs.Controllers
{

    public class AccountsController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AccountsController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IMapper mapper
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        // Login
        [AllowAnonymous]
        [HttpPost("Login")] //POST : /api/accounts/login 
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is null) return Unauthorized(new ApiResponse(401));

            var Result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if(!Result.Succeeded) return Unauthorized(new ApiResponse(401));


            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _tokenService.CreateTokenAsync(user, _userManager),
            });

        }


        // Register
        [HttpPost("Register")] // POST : /api/accounts/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto model)
        {

            if (CheckEmailExists(model.Email).Result.Value)
            {
                return BadRequest(new ApiResponse(400, "email is aleardy exists!"));
            }


            var user = new AppUser()
            {
                Email = model.Email,
                DisplayName = model.DisplayName,
                PhoneNumber = model.PhoneNumber,
                UserName = model.Email.Split("@")[0],
            };

            var Result = await _userManager.CreateAsync(user, model.Password);

            if (!Result.Succeeded) return BadRequest(new ApiResponse(400));

            var ReturnedUser = new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _tokenService.CreateTokenAsync(user,_userManager),
            };
            return Ok(ReturnedUser);

        }

        [Authorize]
        [HttpGet("GetCurrentUser")] // GET : /api/accounts/GetCurrentUser
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
           var userEmail = User.FindFirstValue(ClaimTypes.Email);

           var user = await _userManager.FindByEmailAsync(userEmail);

            return Ok(new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Token = await _tokenService.CreateTokenAsync(user, _userManager),
            });
        }

        [Authorize]
        [HttpGet("address")] // GET: /api/accounts/CurrentUserAddress
        public async Task<ActionResult<AddressDto>> GetCurrentUserAddress()
        {
            //var userEmail = User.FindFirstValue(ClaimTypes.Email);

            //var user = await _userManager.FindByEmailAsync(userEmail);
            var user = await _userManager.FindUserWithAddressAsync(User);
            var MappedAddress = _mapper.Map<Address, AddressDto>(user.Address);
            return Ok(MappedAddress);
        }



        [Authorize]
        [HttpPut("address")] // PUT : /api/account/address
        public async Task<ActionResult<AddressDto>> UpdateCurrentUserAddress(AddressDto model)
        {
            var user = await _userManager.FindUserWithAddressAsync(User);

           var address =  _mapper.Map<AddressDto, Address>(model);

            user.Address = address;

            var Result = await _userManager.UpdateAsync(user);
            if (!Result.Succeeded) return BadRequest(new ApiResponse(400));

            return Ok(model);
        }


        [HttpGet("emailExists")] // GET: /api/accounts/emailExists?email=
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            //var user = await _userManager.FindByEmailAsync(email);
            // if (user is null) return false;
            // return true;


            return await _userManager.FindByEmailAsync(email) is not null;
        }

    }
}
