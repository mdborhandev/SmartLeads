using MediatR;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using IUserRepo = SmartLeads.Utilities.Interfaces.IUserRepository;

namespace SmartLeads.Web.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    string Username,
    string Email,
    string? FirstName = null,
    string? LastName = null) : IRequest<Unit>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public UpdateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IUserRepository userRepository)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.Username);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        // Check if username or email is already taken by another user
        var existingUser = await _userRepository.GetByUsernameOrEmailAsync(request.Username);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            throw new Exception("Username or email already exists.");
        }

        user.Username = request.Username;
        user.Email = request.Email;
        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.UpdatedAt = DateTime.UtcNow;

        _userRepository.Edit(user);
        await _unitOfWork.SaveAsync(cancellationToken);

        return Unit.Value;
    }
}
