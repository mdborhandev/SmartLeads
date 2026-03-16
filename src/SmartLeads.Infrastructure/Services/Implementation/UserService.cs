using Microsoft.Extensions.Configuration;
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
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IJwtTokenGenerator jwtTokenGenerator,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _configuration = configuration;
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string subject, string emailBodyTemplate, string baseUrl)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        
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
        var user = await _userRepository.GetByEmailAsync(email);

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
        var user = await _userRepository.GetByUsernameOrEmailAsync(usernameOrEmail);

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

    public async Task<(bool Success, string? Token, string? Error)> RegisterAsync(string username, string email, string password, string firstName, string lastName)
    {
        // Check if username exists
        var existingUser = await _userRepository.GetByUsernameAsync(username);
        if (existingUser != null)
        {
            return (false, null, "Username already exists.");
        }

        // Check if email exists
        var existingEmail = await _userRepository.GetByEmailAsync(email);
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
            LastName = lastName
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveAsync();

        var token = _jwtTokenGenerator.GenerateToken(user);

        return (true, token, null);
    }

    private string GenerateResetToken()
    {
        return Guid.NewGuid().ToString("N");
    }
}
