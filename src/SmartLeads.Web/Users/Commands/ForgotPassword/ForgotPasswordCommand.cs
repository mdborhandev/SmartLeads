using MediatR;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Web.Users.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Unit>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Unit>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public ForgotPasswordCommandHandler(
        IGenericRepository<User> userRepository,
        IEmailService emailService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.FindAsync(u => u.Email == request.Email);
        var user = users.FirstOrDefault();

        // Always return success to prevent email enumeration
        if (user == null)
        {
            return Unit.Value;
        }

        // Generate reset token
        var resetToken = Guid.NewGuid().ToString("N");
        user.ResetPasswordToken = resetToken;
        user.ResetPasswordTokenExpiryTime = DateTime.UtcNow.AddHours(24);

        await _unitOfWork.SaveAsync(cancellationToken);

        // Send reset email using generic SendEmailAsync
        var resetLink = $"http://localhost:5000/Auth/ResetPassword?token={resetToken}&email={Uri.EscapeDataString(user.Email)}";
        var subject = "Password Reset Request - SmartLeads";
        var body = GeneratePasswordResetEmailBody(user.Username, resetLink);
        await _emailService.SendEmailAsync(user.Email, subject, body);

        return Unit.Value;
    }

    private string GeneratePasswordResetEmailBody(string username, string resetLink)
    {
        return $@"
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
        .button {{ display: inline-block; background: #667eea; color: white; padding: 14px 32px; text-decoration: none; border-radius: 6px; margin: 20px 0; font-weight: bold; }}
        .button:hover {{ background: #5a6fd6; }}
        .footer {{ text-align: center; margin-top: 20px; color: #888; font-size: 12px; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Password Reset Request</h1>
        </div>
        <div class='content'>
            <p>Hello {username},</p>
            <p>We received a request to reset your SmartLeads account password. Click the button below to reset your password:</p>

            <div style='text-align: center;'>
                <a href='{resetLink}' class='button'>Reset Password</a>
            </div>

            <p>Or copy and paste this link into your browser:</p>
            <p style='word-break: break-all; color: #667eea;'>{resetLink}</p>

            <div class='warning'>
                <strong>⚠️ Important:</strong> This link will expire in 24 hours. If you didn't request this password reset, you can safely ignore this email. Your password will remain unchanged.
            </div>

            <p>Best regards,<br><strong>The SmartLeads Team</strong></p>
        </div>
        <div class='footer'>
            <p>&copy; {DateTime.Now.Year} SmartLeads. All rights reserved.</p>
            <p>This is an automated message, please do not reply.</p>
        </div>
    </div>
</body>
</html>";
    }
}
