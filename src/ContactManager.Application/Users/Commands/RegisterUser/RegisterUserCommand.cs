using MediatR;
using ContactManager.Application.Common.Interfaces;
using ContactManager.Application.Common.Models;
using ContactManager.Domain.Interfaces.Repositories;
using ContactManager.Domain.Models;

namespace ContactManager.Application.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string? FirstName = null,
    string? LastName = null) : IRequest<AuthResponse>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterUserCommandHandler(
        IUnitOfWork unitOfWork,
        IGenericRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.FindAsync(u => u.Username == request.Username || u.Email == request.Email);
        if (existingUser.Any())
        {
            throw new Exception("User already exists.");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse(token, user.Username, user.Email);
    }
}
