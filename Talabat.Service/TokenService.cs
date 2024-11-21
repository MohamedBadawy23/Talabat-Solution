using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Identity;
using Talabat.Core.Services.Contract;

namespace Talabat.Service
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> CreateTokenAsync(AppUser user, UserManager<AppUser> userManager)
        {

            // 1. Header [Algo, Type]
            // 2. Payload [Claims]

            var AuthClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.GivenName, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
            };

            var userRoles = await userManager.GetRolesAsync(user);

            foreach (var Role in userRoles)
            {
                AuthClaims.Add(new Claim(ClaimTypes.Role, Role));
            }


            var AuthKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

            // 3. Signature [Key]


            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                claims: AuthClaims,
                expires: DateTime.Now.AddDays(double.Parse(_configuration["JWT:DurationInDays"])),
                signingCredentials: new SigningCredentials(AuthKey, SecurityAlgorithms.HmacSha256Signature)
                );


            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
