using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PharmacyAPI.Models;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Admin.Controllers
{
    [Authorize(Roles = $"{CD.SUPER_ADMIN_ROLE},{CD.ADMIN_ROLE},{CD.PHARMACIST_ROLE}")]
    [Area(CD.ADMIN_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = _userManager.Users.ToList();

            var response = new List<UserResponse>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                response.Add(new UserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    Address = user.Address,
                    Roles = roles.ToList(),

                    IsLocked =
                        user.LockoutEnd.HasValue &&
                        user.LockoutEnd.Value > DateTimeOffset.UtcNow
                });
            }

            return Ok(new ApiResponse<List<UserResponse>>
            {
                IsSuccess = true,
                Message = "Users retrieved successfully",
                Data = response
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            var roles = await _userManager.GetRolesAsync(user);

            var response = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Roles = roles.ToList(),

                IsLocked =
                    user.LockoutEnd.HasValue &&
                    user.LockoutEnd.Value > DateTimeOffset.UtcNow
            };

            return Ok(new ApiResponse<UserResponse>
            {
                IsSuccess = true,
                Message = "User retrieved successfully",
                Data = response
            });
        }

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles
                .Select(r => r.Name!)
                .ToList();

            return Ok(new ApiResponse<List<string>>
            {
                IsSuccess = true,
                Message = "Roles retrieved successfully",
                Data = roles
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address
            };

            var result = await _userManager.CreateAsync(
                user,
                request.Password
            );

            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)
                    )
                });
            }

            if (request.SelectedRoles.Any())
            {
                var roleResult = await _userManager.AddToRolesAsync(
                    user,
                    request.SelectedRoles
                );

                if (!roleResult.Succeeded)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = string.Join(
                            ", ",
                            roleResult.Errors.Select(e => e.Description)
                        )
                    });
                }
            }

            var roles = await _userManager.GetRolesAsync(user);

            var response = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Roles = roles.ToList(),
                IsLocked = false
            };

            return Ok(new ApiResponse<UserResponse>
            {
                IsSuccess = true,
                Message = "User created successfully",
                Data = response
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(
    string id,
    UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.UserName = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            user.Address = request.Address;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description)
                    )
                });
            }

            var currentRoles =
                await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles
                );
            }

            if (request.SelectedRoles.Any())
            {
                await _userManager.AddToRolesAsync(
                    user,
                    request.SelectedRoles
                );
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                var token =
                    await _userManager.GeneratePasswordResetTokenAsync(user);

                var passwordResult =
                    await _userManager.ResetPasswordAsync(
                        user,
                        token,
                        request.Password
                    );

                if (!passwordResult.Succeeded)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        IsSuccess = false,
                        Message = string.Join(
                            ", ",
                            passwordResult.Errors.Select(e => e.Description)
                        )
                    });
                }
            }

            var roles = await _userManager.GetRolesAsync(user);

            var response = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Roles = roles.ToList(),

                IsLocked =
                    user.LockoutEnd.HasValue &&
                    user.LockoutEnd.Value > DateTimeOffset.UtcNow
            };

            return Ok(new ApiResponse<UserResponse>
            {
                IsSuccess = true,
                Message = "User updated successfully",
                Data = response
            });
        }

        [HttpPut("{id}/lock")]
        public async Task<IActionResult> Lock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            await _userManager.SetLockoutEnabledAsync(user, true);

            await _userManager.SetLockoutEndDateAsync(
                user,
                DateTimeOffset.UtcNow.AddYears(100)
            );

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "User locked successfully"
            });
        }

        [HttpPut("{id}/unlock")]
        public async Task<IActionResult> Unlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound(new ApiResponse<object>
                {
                    IsSuccess = false,
                    Message = "User not found"
                });
            }

            await _userManager.SetLockoutEndDateAsync(
                user,
                null
            );

            return Ok(new ApiResponse<object>
            {
                IsSuccess = true,
                Message = "User unlocked successfully"
            });
        }
    }
}
