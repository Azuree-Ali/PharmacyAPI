using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PharmacyAPI.Models;

//using Stripe;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PharmacyAPI.JwtFeatures
{
    public class JwtHandler : IJwtHandler
    {
        private readonly IConfiguration _configuration; 
        private readonly IConfigurationSection _configurationSection; 
        private readonly UserManager<ApplicationUser> _userManager;
        public JwtHandler(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _configurationSection = _configuration.GetSection("JwtSettings");
            _userManager = userManager;
        }

        public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configurationSection["SecretKey"])); 
            var SigningCredentials  = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var calims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name , user.UserName)  , 
                new Claim(ClaimTypes.Email, user.Email)  , 
                new Claim(ClaimTypes.NameIdentifier , user.Id) 
            };
            var roles = await _userManager.GetRolesAsync(user);
            foreach(var role in roles)
            {
                calims.Add(new Claim(ClaimTypes.Role , role)); 
            }
            

            var jwtSecurityToken = new JwtSecurityToken(
                    issuer: _configurationSection["ValidIssuer"],
                    audience: _configurationSection["ValidAudience"],
                    claims: calims,
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configurationSection["ExpirationTimeInMins"])) , 
                    signingCredentials: SigningCredentials
                );  
            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken); 
        }
    }
}
