using MediatR;
using SmartLeads.Application.Common.Interfaces;
using SmartLeads.Domain.Interfaces.Repositories;
using SmartLeads.Domain.Models;

namespace SmartLeads.Application.Users.Commands.ForgotPassword;

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

        // Send reset email
        var resetLink = $"http://localhost:5000/Auth/ResetPassword?token={resetToken}&email={Uri.EscapeDataString(user.Email)}";
        await _emailService.SendPasswordResetEmailAsync(user.Email, user.Username, resetLink);

        return Unit.Value;
    }
}
