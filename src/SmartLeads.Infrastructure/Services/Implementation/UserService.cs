using Microsoft.Extensions.Configuration;
using SmartLeads.Domain.Enums;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Infrastructure.Services.Interface;
using SmartLeads.Utilities.Email;
using SmartLeads.Utilities.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SmartLeads.Infrastructure.Services.Implementation;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;

    public UserService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IJwtTokenGenerator jwtTokenGenerator,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string subject, string emailBodyTemplate, string baseUrl)
    {
        var user = await _unitOfWork.userRepository.GetByEmailAsync(email);
        
        // Always return true to prevent email enumeration
        if (user == null)
        {
            return true;
        }

        // Generate reset token
        var resetToken = GenerateResetToken();
        user.ResetPasswordToken = resetToken;
        user.ResetPasswordTokenExpiryTime = DateTime.UtcNow.AddHours(24);

        await _unitOfWork.SaveAsync();

        // Generate reset link
        var resetLink = $"{baseUrl}/Auth/ResetPassword?token={resetToken}&email={Uri.EscapeDataString(user.Email)}";
        
        // Replace placeholder in template with actual reset link
        var emailBody = emailBodyTemplate.Replace("{RESET_LINK}", resetLink).Replace("{USERNAME}", user.Username);
        
        // Send email using generic SendEmailAsync
        await _emailService.SendEmailAsync(user.Email, subject, emailBody);

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _unitOfWork.userRepository.GetByEmailAsync(email);

        if (user == null)
        {
            return false;
        }

        // Validate token
        if (user.ResetPasswordToken != token)
        {
            return false;
        }

        // Check token expiry
        if (user.ResetPasswordTokenExpiryTime < DateTime.UtcNow)
        {
            return false;
        }

        // Update password
        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpiryTime = null;

        await _unitOfWork.SaveAsync();

        return true;
    }

    public async Task<(bool Success, string? Token, string? Error)> LoginAsync(string usernameOrEmail, string password)
    {
        // Check for superadmin credentials (hardcoded)
        // Username: superadmin, Password: SuperAdmin@123
        if ((usernameOrEmail == "superadmin" || usernameOrEmail == "superadmin@smartleads.com") 
            && password == "SuperAdmin@123")
        {
            // Create a superadmin user object (not from database)
            var superAdminUser = new User
            {
                Id = Guid.Empty,
                Username = "superadmin",
                Email = "superadmin@smartleads.com",
                FirstName = "Super",
                LastName = "Admin"
            };

            var superAdminToken = _jwtTokenGenerator.GenerateToken(superAdminUser);
            return (true, superAdminToken, null);
        }

        // Regular user login
        var user = await _unitOfWork.userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);

        if (user == null)
        {
            return (false, null, "Invalid username/email or password.");
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return (false, null, "Invalid username/email or password.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return (true, token, null);
    }

    public async Task<(bool Success, string? Token, string? Error)> RegisterAsync(string username, string email, string password, string firstName, string lastName, Guid? companyId = null, UserRole role = UserRole.User)
    {
        // Check if username exists in this company (if companyId is provided)
        if (companyId.HasValue)
        {
            var existingUser = await _unitOfWork.userRepository.GetByUsernameAndCompanyIdAsync(username, companyId.Value);
            if (existingUser != null)
            {
                return (false, null, "Username already exists in this company.");
            }
        }
        else
        {
            // For global check (no company)
            var existingUser = await _unitOfWork.userRepository.GetByUsernameAsync(username);
            if (existingUser != null)
            {
                return (false, null, "Username already exists.");
            }
        }

        // Check if email exists
        var existingEmail = await _unitOfWork.userRepository.GetByEmailAsync(email);
        if (existingEmail != null)
        {
            return (false, null, "Email already exists.");
        }

        var user = new Domain.Models.User
        {
            Username = username,
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password),
            FirstName = firstName,
            LastName = lastName,
            CompanyId = companyId,
            Role = role
        };

        await _unitOfWork.userRepository.AddAsync(user);
        await _unitOfWork.SaveAsync();

        var token = _jwtTokenGenerator.GenerateToken(user);

        return (true, token, null);
    }

    public async Task<Domain.Models.User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
    {
        return await _unitOfWork.userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);
    }

    public async Task<bool> UpdateProfileAsync(string username, string email, string firstName, string lastName)
    {
        var user = await _unitOfWork.userRepository.GetByUsernameOrEmailAsync(username);
        
        if (user == null)
        {
            return false;
        }

        user.Email = email;
        user.FirstName = firstName;
        user.LastName = lastName;
        user.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveAsync();
        
        return true;
    }

    private string GenerateResetToken()
    {
        return Guid.NewGuid().ToString("N");
    }
}
