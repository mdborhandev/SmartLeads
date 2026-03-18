using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SmartLeads.Domain.DTOs;
using SmartLeads.Infrastructure.Services.Interface;

namespace SmartLeads.Web.Controllers;

public class AuthController : Controller
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;

    public AuthController(IUserService userService, IConfiguration configuration)
    {
        _userService = userService;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _userService.LoginAsync(model.EmailOrUsername, model.Password);

            if (result.Success)
            {
                HttpContext.Response.Cookies.Append("JwtToken", result.Token!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

                // Get user details to store CompanyId and UserId in cookies
                var user = await _userService.GetUserByUsernameOrEmailAsync(model.EmailOrUsername);
                if (user != null && user.CompanyId.HasValue)
                {
                    // Store UserId in cookie
                    HttpContext.Response.Cookies.Append("UserId", user.Id.ToString(), new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });

                    // Store CompanyId in cookie
                    HttpContext.Response.Cookies.Append("CompanyId", user.CompanyId.Value.ToString(), new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(1)
                    });
                }

                // Redirect to contacts
                return RedirectToAction("Index", "Contacts");
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.Error);
                return View(model);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    public IActionResult Logout()
    {
        HttpContext.Response.Cookies.Delete("JwtToken");
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction("Login");
        }

        try
        {
            var usernameOrEmail = User.Identity?.Name;
            if (string.IsNullOrEmpty(usernameOrEmail))
            {
                return RedirectToAction("Login");
            }

            // Get user from repository
            var user = await _userService.GetUserByUsernameOrEmailAsync(usernameOrEmail);

            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new UserProfileViewModel
            {
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return View(model);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Login");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(UserProfileViewModel model)
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            return RedirectToAction("Login");
        }

        if (ModelState.IsValid)
        {
            // Get current username (immutable)
            var currentUsername = User.Identity?.Name;
            
            // Update profile
            var success = await _userService.UpdateProfileAsync(
                currentUsername,
                model.Email,
                model.FirstName,
                model.LastName
            );

            if (success)
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update profile.";
            }
            
            return RedirectToAction("Profile");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string token, string email)
    {
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
        {
            return RedirectToAction("Login");
        }

        var model = new ResetPasswordViewModel
        {
            Token = token,
            Email = email
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var success = await _userService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);

            if (success)
            {
                TempData["SuccessMessage"] = "Your password has been reset successfully. You can now log in.";
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid or expired reset token.");
                return View(model);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    #region API Endpoints for AJAX

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiLogin([FromBody] LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Invalid input" });
        }

        try
        {
            var result = await _userService.LoginAsync(model.EmailOrUsername, model.Password);

            if (result.Success)
            {
                HttpContext.Response.Cookies.Append("JwtToken", result.Token!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

                // Get user details
                var user = await _userService.GetUserByUsernameOrEmailAsync(model.EmailOrUsername);
                string? userId = null;
                string? companyId = null;

                if (user != null)
                {
                    userId = user.Id.ToString();
                    companyId = user.CompanyId?.ToString();

                    // Store in cookies if company exists
                    if (user.CompanyId.HasValue)
                    {
                        HttpContext.Response.Cookies.Append("UserId", userId, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddHours(1)
                        });

                        HttpContext.Response.Cookies.Append("CompanyId", companyId, new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddHours(1)
                        });
                    }
                }

                return Ok(new {
                    success = true,
                    message = "Login successful",
                    redirectUrl = Url.Action("Index", "Contacts"),
                    userId = userId,
                    companyId = companyId
                });
            }
            else
            {
                return BadRequest(new { success = false, message = result.Error });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiForgotPassword([FromBody] ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Invalid email address" });
        }

        try
        {
            // The service will generate the token and send the email
            // Controller passes subject and email body template
            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
            var subject = "Password Reset Request - SmartLeads";
            
            // Email body template (token will be inserted by service)
            var emailBodyTemplate = GetPasswordResetEmailTemplate();
            
            // For now, we'll use a simplified approach - service handles token generation
            // and email sending with the provided template
            await _userService.SendPasswordResetEmailAsync(model.Email, subject, emailBodyTemplate, baseUrl);

            // Always return success to prevent email enumeration
            return Ok(new { success = true, message = "If an account exists with that email, we've sent a password reset link." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApiResetPassword([FromBody] ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new { success = false, message = "Invalid input" });
        }

        try
        {
            var success = await _userService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);

            if (success)
            {
                return Ok(new { success = true, message = "Your password has been reset successfully." });
            }
            else
            {
                return BadRequest(new { success = false, message = "Invalid or expired reset token." });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    #endregion

    #region Helper Methods

    private string GetPasswordResetEmailTemplate()
    {
        return @"
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }
        .content { background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }
        .button { display: inline-block; background: #667eea; color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; margin: 20px 0; font-weight: bold; }
        .button:hover { background: #5a6fd6; }
        .footer { text-align: center; margin-top: 20px; color: #888; font-size: 12px; }
        .warning { background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hello {USERNAME},</p>
            <p>We received a request to reset your SmartLeads account password. Click the button below to reset your password:</p>

            <div style='text-align: center;'>
                <a href='{RESET_LINK}' class='button'>Reset Password</a>
            </div>

            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #667eea;'>{RESET_LINK}</p>

            <div class='warning'>
                <strong>⚠️ Important:</strong> This link will expire in 24 hours. If you didn't request this password reset, you can safely ignore this email. Your password will remain unchanged.
            </div>

            <p>Best regards,<br><strong>The SmartLeads Team</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; " + DateTime.Now.Year + @" SmartLeads. All rights reserved.</p>
            <p>This is an automated message, please do not reply.</p>
        </div>
    </div>
</body>
</html>";
    }

    #endregion
}
