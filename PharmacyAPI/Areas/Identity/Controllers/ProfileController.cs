using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.Models;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Identity.Controllers
{
    [Route("api/[area]/[controller]")]
    [Area(CD.IDENTITY_AREA)]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _usermanager;

        public ProfileController(UserManager<ApplicationUser> usermanager)
        {
            _usermanager = usermanager;
        }
        [HttpGet]
        public async Task<IActionResult> GetInfo()
        {
            var user = await _usermanager.GetUserAsync(User);
            if(user == null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "User not found",
                    Error = "User not found"
                });
            }
            var userResponse = new ApplicationUserResponse
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Adresse = user.Address,
                Email = user.Email


            };

            return Ok(new ApiResponse<ApplicationUserResponse>()
            {
                IsSuccess = true,
                Message = "User profile retrieved successfully",
                Data = userResponse
            });


        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateProfile(ApplicationUserRequest applicationUserRequest)
        {
            var user = await _usermanager.GetUserAsync(User);
            user.FirstName = applicationUserRequest.FirstName;
            user.LastName = applicationUserRequest.LastName;
            user.PhoneNumber = applicationUserRequest.PhoneNumber;
            user.Address = applicationUserRequest.Adresse;
            user.Email = applicationUserRequest.Email;
            var result = await _usermanager.UpdateAsync(user);
            if(user == null)
            {
                return NotFound(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "User not found",
                    Error = "User not found"
                });
            }
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "Failed to update user profile",
                    Error = string.Join(", ", result.Errors.Select(e => e.Description))
                });
            }
            else
            {
                return Ok(new ApiResponse<object>()
                {
                    IsSuccess = true,
                    Message = "User profile updated successfully",

                });
            }


        }
        //f
        [HttpPut("update-password")]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordRequest updatePasswordRequest)
        {
            var user = await _usermanager.GetUserAsync(User);
            var result = await _usermanager.ChangePasswordAsync(user, updatePasswordRequest.CurrentPassword, updatePasswordRequest.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>()
                {
                    IsSuccess = false,
                    Message = "Failed to update password",
                    Error = string.Join(", ", result.Errors.Select(e => e.Description))
                });
            }
            else
            {
                return Ok(new ApiResponse<object>()
                {
                    IsSuccess = true,
                    Message = "Password updated successfully",
                });
            }


        }
    }
}
