using MediatR;
using SmartLeads.Application.Common.Interfaces;
using SmartLeads.Domain.Interfaces.Repositories;
using SmartLeads.Domain.Models;

namespace SmartLeads.Application.Users.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<Unit>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Unit>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(
        IGenericRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.FindAsync(u => u.Email == request.Email);
        var user = users.FirstOrDefault();

        if (user == null)
        {
            throw new Exception("Invalid reset token.");
        }

        // Validate token
        if (user.ResetPasswordToken != request.Token)
        {
            throw new Exception("Invalid reset token.");
        }

        // Check token expiry
        if (user.ResetPasswordTokenExpiryTime < DateTime.UtcNow)
        {
            throw new Exception("Reset token has expired.");
        }

        // Update password
        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        user.ResetPasswordToken = null;
        user.ResetPasswordTokenExpiryTime = null;

        await _unitOfWork.SaveAsync(cancellationToken);

        return Unit.Value;
    }
}
