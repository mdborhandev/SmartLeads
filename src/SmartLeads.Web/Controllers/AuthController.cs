using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Enums;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Infrastructure.Services;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Web.Controllers;

public class AuthController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ICompanyContext _companyContext;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public AuthController(IUnitOfWork unitOfWork, IConfiguration configuration, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, ICompanyContext companyContext, IWebHostEnvironment webHostEnvironment)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _companyContext = companyContext;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public IActionResult Register()
    {
        // If user is already logged in, redirect to dashboard
        if (User.Identity?.IsAuthenticated == true)
        {
return RedirectToAction("Index", "Home");
         }
         
         return View();
     }

     [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Check if username exists
            var existingUsername = await _unitOfWork.userRepository.GetByUsernameAsync(model.Username);
            if (existingUsername != null)
            {
                ModelState.AddModelError(string.Empty, "Username already exists.");
                return View(model);
            }

            // Check if email exists
            var existingEmail = await _unitOfWork.userRepository.GetByEmailAsync(model.Email);
            if (existingEmail != null)
            {
                ModelState.AddModelError(string.Empty, "Email already exists.");
                return View(model);
            }

            // Create user
            var user = new Domain.Models.User
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = _passwordHasher.HashPassword(model.Password),
                FirstName = model.FirstName,
                LastName = model.LastName
                // Role is now stored in UserCompany, not in User
            };

            await _unitOfWork.userRepository.AddAsync(user);
            await _unitOfWork.SaveAsync();

            // Generate JWT token (default role for new registration without company)
            var token = _jwtTokenGenerator.GenerateToken(user, Domain.Enums.UserRole.User);

            // Auto login after registration
            HttpContext.Response.Cookies.Append("JwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            // Store UserId in cookie
            HttpContext.Response.Cookies.Append("UserId", user.Id.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            // Redirect to UserCompany/CreateCompany page since user doesn't have a company yet
            return RedirectToAction("CreateCompany", "UserCompany");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
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
            var user = await _unitOfWork.userRepository.GetByUsernameOrEmailAsync(model.EmailOrUsername);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
                return View(model);
            }

            // Verify password
            var isValidPassword = await _unitOfWork.userRepository.VerifyPasswordAsync(model.Password, user.PasswordHash);
            if (!isValidPassword)
            {
                ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
                return View(model);
            }

            // Check if user has any company association
            var hasCompany = await _unitOfWork.userRepository.GetUserCompaniesAsync(user.Id);
            if (hasCompany == null || !hasCompany.Any())
            {
                // User has no company - set authentication cookies and redirect to NoCompany page
                // Generate JWT token with default User role (no company context)
                var loginToken = _jwtTokenGenerator.GenerateToken(user, Domain.Enums.UserRole.User);

                HttpContext.Response.Cookies.Append("JwtToken", loginToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

                // Store UserId in cookie
                HttpContext.Response.Cookies.Append("UserId", user.Id.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });

                // Redirect to CreateCompany page
                return RedirectToAction("CreateCompany", "UserCompany");
            }

            // Get default company
            var defaultCompany = hasCompany.FirstOrDefault(uc => uc.IsDefault && uc.IsActive && !uc.IsDeleted)
                ?? hasCompany.FirstOrDefault(uc => uc.IsActive && !uc.IsDeleted);

            if (defaultCompany == null)
            {
                ModelState.AddModelError(string.Empty, "No active company association found.");
                return View(model);
            }

            // Get role from UserCompany for the default company
            var userRole = defaultCompany.Role;

            // Generate JWT token with the role from UserCompany and CompanyId claim
            var token = _jwtTokenGenerator.GenerateToken(user, userRole, defaultCompany.CompanyId);

            HttpContext.Response.Cookies.Append("JwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            // Store UserId in cookie
            HttpContext.Response.Cookies.Append("UserId", user.Id.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            // Set default company in session and cookie
            HttpContext.Session.SetString("CurrentCompanyId", defaultCompany.CompanyId.ToString());

            // Store Company ID in cookie
            HttpContext.Response.Cookies.Append("CurrentCompanyId", defaultCompany.CompanyId.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            // Get Employee record for this user in this company and store in cookie
            var employeeUser = await _unitOfWork.defaultDbContext.EmployeeUsers
                .Include(eu => eu.Employee)
                .FirstOrDefaultAsync(eu => eu.UserId == user.Id && eu.Employee.CompanyId == defaultCompany.CompanyId);

            if (employeeUser != null && employeeUser.Employee != null)
            {
                HttpContext.Response.Cookies.Append("CurrentEmployeeId", employeeUser.EmployeeId.ToString(), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(1)
                });
            }

            // Check layout preference and redirect accordingly
            var useUserCompanyLayout = HttpContext.Session.GetString("UseUserCompanyLayout") == "true";
            
            if (useUserCompanyLayout)
            {
                // User prefers UserCompany layout, redirect to Dashboard
                return RedirectToAction("Dashboard", "UserCompany");
            }

// Redirect to home (Main layout)
             return RedirectToAction("Index", "Home");
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
        // Delete all cookies
        HttpContext.Response.Cookies.Delete("JwtToken");
        HttpContext.Response.Cookies.Delete("UserId");
        HttpContext.Response.Cookies.Delete("CurrentCompanyId");
        HttpContext.Response.Cookies.Delete("CurrentEmployeeId");
        HttpContext.Response.Cookies.Delete("SmartLeads.Session");
        
        // Delete antiforgery cookie (pattern matches .AspNetCore.Antiforgery.*)
        var responseCookies = HttpContext.Response.Cookies;
        foreach (var cookie in HttpContext.Request.Cookies.Keys)
        {
            if (cookie.StartsWith(".AspNetCore.Antiforgery", StringComparison.OrdinalIgnoreCase))
            {
                responseCookies.Delete(cookie);
            }
        }
        
        // Clear and abandon session
        HttpContext.Session.Clear();
        
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
            var user = await _unitOfWork.userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);

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
                ProfilePicture = user.ProfilePicture,
                // Get role from current company's UserCompany
                Role = await _companyContext.GetCurrentCompanyRoleAsync() ?? UserRole.User,
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

        // Get current user
        var usernameOrEmail = User.Identity?.Name;
        var user = await _unitOfWork.userRepository.GetByUsernameOrEmailAsync(usernameOrEmail ?? "");

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction("Login");
        }

        // Validate username uniqueness (globally, not company-specific)
        if (!string.Equals(user.Username, model.Username, StringComparison.OrdinalIgnoreCase))
        {
            var isUsernameTaken = await _unitOfWork.userRepository.IsUsernameTakenAsync(model.Username, user.Id);
            if (isUsernameTaken)
            {
                ModelState.AddModelError(nameof(model.Username), "This username is already taken by another user.");
            }
        }

        // Validate email uniqueness (globally, not company-specific)
        if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
        {
            var isEmailTaken = await _unitOfWork.userRepository.IsEmailTakenAsync(model.Email, user.Id);
            if (isEmailTaken)
            {
                ModelState.AddModelError(nameof(model.Email), "This email address is already registered by another user.");
            }
        }

        // Handle profile picture upload
        var profilePictureFile = Request.Form.Files.GetFile("ProfilePictureFile");
        if (profilePictureFile != null && profilePictureFile.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(profilePictureFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
            {
                ModelState.AddModelError("ProfilePictureFile", "Only image files (jpg, jpeg, png, gif, webp) are allowed.");
            }
            else if (profilePictureFile.Length > 5 * 1024 * 1024) // 5MB limit
            {
                ModelState.AddModelError("ProfilePictureFile", "Profile picture must be less than 5MB.");
            }
            else
            {
                try
                {
                    // Use existing storage/uploads folder (at solution root: /SmartLeads/storage/uploads)
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "storage", "uploads");
                    Directory.CreateDirectory(uploadsPath);

                    // Generate unique filename
                    var fileName = $"{user.Id}_{Guid.NewGuid():N}{fileExtension}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    // Save file
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profilePictureFile.CopyToAsync(stream);
                    }

                    // Delete old profile picture if exists
                    if (!string.IsNullOrEmpty(user.ProfilePicture) && user.ProfilePicture.StartsWith("/storage/uploads/"))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "storage", "uploads", Path.GetFileName(user.ProfilePicture));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Update profile picture path (relative to wwwroot for serving)
                    user.ProfilePicture = $"/storage/uploads/{fileName}";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ProfilePictureFile", $"Error uploading image: {ex.Message}");
                }
            }
        }

        // Handle password change (only if password fields are provided)
        bool passwordChanged = false;
        if (!string.IsNullOrEmpty(model.CurrentPassword) || !string.IsNullOrEmpty(model.NewPassword) || !string.IsNullOrEmpty(model.ConfirmNewPassword))
        {
            // If any password field is filled, validate all
            if (string.IsNullOrEmpty(model.CurrentPassword))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is required to change password.");
            }
            else if (string.IsNullOrEmpty(model.NewPassword))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "New password is required.");
            }
            else if (string.IsNullOrEmpty(model.ConfirmNewPassword))
            {
                ModelState.AddModelError(nameof(model.ConfirmNewPassword), "Please confirm your new password.");
            }
            else
            {
                // Verify current password
                var isCurrentPasswordValid = await _unitOfWork.userRepository.VerifyPasswordAsync(model.CurrentPassword, user.PasswordHash);
                if (!isCurrentPasswordValid)
                {
                    ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is incorrect.");
                }
                else
                {
                    // Hash new password and update
                    var newPasswordHash = _passwordHasher.HashPassword(model.NewPassword);
                    passwordChanged = await _unitOfWork.userRepository.ChangePasswordAsync(user.Id, user.PasswordHash, newPasswordHash);

                    if (!passwordChanged)
                    {
                        ModelState.AddModelError(string.Empty, "Failed to change password. Please try again.");
                    }
                }
            }
        }

        if (!ModelState.IsValid)
        {
            // Reload readonly fields for the view
            model.Role = await _companyContext.GetCurrentCompanyRoleAsync() ?? UserRole.User;
            model.CreatedAt = user.CreatedAt;
            model.UpdatedAt = user.UpdatedAt;
            model.ProfilePicture = user.ProfilePicture;
            return View(model);
        }

        try
        {
            // Update user profile
            user.Username = model.Username;
            user.Email = model.Email;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.userRepository.UpdateProfileAsync(user);
            
            if (passwordChanged)
            {
                TempData["SuccessMessage"] = "Profile and password updated successfully!";
            }
            else
            {
                TempData["SuccessMessage"] = "Profile updated successfully!";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction("Profile");
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
            var success = await _unitOfWork.userRepository.ResetPasswordAsync(model.Email, model.Token, _passwordHasher.HashPassword(model.NewPassword));

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
            var user = await _unitOfWork.userRepository.GetByUsernameOrEmailAsync(model.EmailOrUsername);

            if (user == null)
            {
                return BadRequest(new { success = false, message = "Invalid username/email or password." });
            }

            // Verify password
            var isValidPassword = await _unitOfWork.userRepository.VerifyPasswordAsync(model.Password, user.PasswordHash);
            if (!isValidPassword)
            {
                return BadRequest(new { success = false, message = "Invalid username/email or password." });
            }

            // Generate JWT token
            var token = _jwtTokenGenerator.GenerateToken(user);

            HttpContext.Response.Cookies.Append("JwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            // Store UserId in cookie
            HttpContext.Response.Cookies.Append("UserId", user.Id.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });

            // Check if user has any company association
            var hasCompany = await _unitOfWork.userRepository.GetUserCompaniesAsync(user.Id);
            if (hasCompany == null || !hasCompany.Any())
            {
                // Redirect to CreateCompany page
                return Ok(new {
                    success = true,
                    message = "Login successful",
                    redirectUrl = Url.Action("CreateCompany", "UserCompany"),
                    userId = user.Id.ToString()
                });
            }

            return Ok(new {
                success = true,
                message = "Login successful",
                redirectUrl = Url.Action("Index", "Home"),
                userId = user.Id.ToString()
            });
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
            // Generate reset token
            var resetToken = Guid.NewGuid().ToString("N");
            var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
            
            // Set token in database
            await _unitOfWork.userRepository.SetPasswordResetTokenAsync(model.Email, resetToken, DateTime.UtcNow.AddHours(24));

            // Generate reset link
            var resetLink = $"{baseUrl}/Auth/ResetPassword?token={resetToken}&email={Uri.EscapeDataString(model.Email)}";

            // Replace placeholder in template with actual reset link
            var emailBody = GetPasswordResetEmailTemplate().Replace("{RESET_LINK}", resetLink);

            // Send email using email service
            var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();
            await emailService.SendEmailAsync(model.Email, "Password Reset Request - SmartLeads", emailBody);

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
            var success = await _unitOfWork.userRepository.ResetPasswordAsync(model.Email, model.Token, _passwordHasher.HashPassword(model.NewPassword));

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
