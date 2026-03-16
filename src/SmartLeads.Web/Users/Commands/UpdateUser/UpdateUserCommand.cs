using MediatR;
using SmartLeads.Domain.Interfaces.Services;
using SmartLeads.Domain.Interfaces.Repositories;
using SmartLeads.Domain.Models;

namespace SmartLeads.Web.Users.Commands.UpdateUser;

public record UpdateUserCommand(
    string Username,
    string Email,
    string? FirstName = null,
    string? LastName = null) : IRequest<Unit>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<User> _userRepository;

    public UpdateUserCommandHandler(
        IUnitOfWork unitOfWork,
        IGenericRepository<User> userRepository)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.FindAsync(u => u.Username == request.Username || u.Email == request.Email);
        var user = users.FirstOrDefault(u => u.Username == request.Username || u.Email == request.Email);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        // Check if username or email is already taken by another user
        var existingUser = await _userRepository.FindAsync(u => 
            (u.Username == request.Username && u.Id != user.Id) ||
            (u.Email == request.Email && u.Id != user.Id));

        if (existingUser.Any())
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
