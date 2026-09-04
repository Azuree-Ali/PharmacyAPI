using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Pharmacy.DTOs.Request;
using PharmacyAPI.Models;
using PharmacyAPI.Repositories;
using PharmacyAPI.Utils;

namespace PharmacyAPI.Areas.Identity.Controllers
{
    [Area(CD.IDENTITY_AREA)]
    [Route("api/[area]/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IRepository<ApplicationUserOtp> _applicationUserOtpRepository;
        private readonly IEmailSender _emailSender;
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IRepository<ApplicationUserOtp> applicationUserOtpRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOtpRepository = applicationUserOtpRepository;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest registerRequest)
        {
            var user = new ApplicationUser
            {
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                UserName = registerRequest.Username,
                Email = registerRequest.Email
            };
            var result = await _userManager.CreateAsync(user, registerRequest.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object> (){ IsSuccess = false, Message = "Failed to create user", Error = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            await _userManager.AddToRoleAsync(user, CD.CUSTOMER_ROLE);
            // Send Email 

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = CD.IDENTITY_AREA, userId = user.Id, token = token }, Request.Scheme);

            await _emailSender.SendEmailAsync(
                registerRequest.Email,
                "Ecommerce confirm Email",
                $"<h1>Please click <a href={link}>here</a> to Confirm Your Mail</h1>"
                );
            return Ok(new ApiResponse<object>()
            {
                IsSuccess = true,
                Message = "User created successfully"
            });
        }
        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByEmailAsync(loginRequest.UsernameOrEmail) ??
                        await _userManager.FindByNameAsync(loginRequest.UsernameOrEmail);
            if (user is null)
            {
                return NotFound(new ApiResponse<object>() { IsSuccess = false, Message = "Invalid UserName or Password" });
            }
            var result = await _signInManager.PasswordSignInAsync(user, loginRequest.Password, loginRequest.RememberMe, true);
            var errors = new List<string>();
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    errors.Add("user is Locked Please try again Later");
                }
                else if (result.IsNotAllowed)
                {
                    errors.Add("please Confirm Your Email First");
                }
                else
                {
                    errors.Add("Invalid UserName or Password");
                }
                return BadRequest(new ApiResponse<object> { IsSuccess = false, Message = "Invalid Data", Error = string.Join(", ", errors) });
            }
            return Ok(new ApiResponse<object>() { IsSuccess = true, Message = "Login successful" });
        }
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return NotFound(new ApiResponse<object>() { IsSuccess = false, Message = "Invalid User" });

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>() { IsSuccess = false, Message = "Email Confirmation Failed", Error = string.Join(", ", result.Errors.Select(e => e.Description)) });
            }
            return Ok(new ApiResponse<object>() { IsSuccess = true, Message = "Email Confirmed Successfully" });
        }

        [HttpPost("ResendEmailConfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationRequest resendEmailConfirmationRequest)
        {
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationRequest.UserNameOrEmail) ??
                       await _userManager.FindByNameAsync(resendEmailConfirmationRequest.UserNameOrEmail);
            if (user is null)
            {
                return NotFound(new ApiResponse<object>() { IsSuccess = false, Message = "Invalid UserName or Password" });
            }
            // Send Email 
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action(nameof(ConfirmEmail), "Account", new { area = CD.IDENTITY_AREA, userId = user.Id, token = token }, Request.Scheme);
            await _emailSender.SendEmailAsync(
                user.Email,
                "Ecommerce confirm Email",
                $"<h1>Please click <a href={link}>here</a> to Confirm Your Mail</h1>"
                );
            return Ok(new ApiResponse<object>() { IsSuccess = true, Message = "Email Confirmation Sent Successfully" });
        }
        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest forgetPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(forgetPasswordRequest.UserNameOrEmail) ??
                await _userManager.FindByNameAsync(forgetPasswordRequest.UserNameOrEmail);

            if (user is null)
            {
                return BadRequest(new ApiResponse<object>() { IsSuccess = false, Message = "Invalid UserName or Email" });
            }
            var otp = new Random().Next(1000, 9999).ToString();
            var applicationUserOtp = new ApplicationUserOtp(user.Id, otp);
            await _applicationUserOtpRepository.CreateAsync(applicationUserOtp);
            await _applicationUserOtpRepository.CommitAsync();
            // send email 
            await _emailSender.SendEmailAsync(
               user.Email,
               "Ecommerce Reset Password",
               $"<h1>use this  <span style=\"color:red\">{otp}</span> as a otp to reset your password </h1>"
               );
            return Ok(new ApiResponse<object>() { IsSuccess = true, Message = "OTP sent to your email successfully" });
        }
        [HttpPost("VerifyOTP")]
        public async Task<IActionResult> VerifyOTP(VerifyOTPRequest verifyOTPRequest)
        {
            var user = await _userManager.FindByIdAsync(verifyOTPRequest.UserId);
            if (user is null)
            {
                return NotFound(new ApiResponse<object>() { IsSuccess = false, Message = "Invalid User" });
            }
            var otps = await _applicationUserOtpRepository.GetAllAsync(e =>
                e.ApplicationUserId == user.Id &&
                e.IsValid &&
                e.ValidTo > DateTime.UtcNow
            );
            var otp = otps.OrderBy(e => e.CreatedAt).LastOrDefault();
            if (otp is null || otp.OTP != verifyOTPRequest.OTP)
            {
                return BadRequest(new ApiResponse<object>() { IsSuccess = false, Message = "invalid / Expired OTP" });
            }
            otp.IsValid = false;
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _applicationUserOtpRepository.CommitAsync();
            return Ok(new ApiResponse<object>() { IsSuccess = true, Message = "OTP verified successfully" , Data = new { Token = token
             , 
            userId = user.Id
            } });
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequest resetPasswordRequest)
        {
            var user = await _userManager.FindByIdAsync(resetPasswordRequest.UserId);
            if (user is null)
            {
                return NotFound(new ApiResponse<object>() { IsSuccess = false, Message = "invalid user" });
            }
            var result = await _userManager.ResetPasswordAsync(user, resetPasswordRequest.Token, resetPasswordRequest.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest(new ApiResponse<object>() { IsSuccess = false, Message = "Failed to reset password" });
            }
            return Ok(new ApiResponse<object>() { IsSuccess = true, Message = "Password reset successfully" });
        }
    }
}
