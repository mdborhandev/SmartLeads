using MediatR;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Web.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string? FirstName = null,
    string? LastName = null) : IRequest<AuthResponse>;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, AuthResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterUserCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        // Check if username exists
        var existingUsername = await _unitOfWork.userRepository.GetByUsernameAsync(request.Username);
        if (existingUsername != null)
        {
            throw new Exception("Username already exists.");
        }

        // Check if email exists
        var existingEmail = await _unitOfWork.userRepository.GetByEmailAsync(request.Email);
        if (existingEmail != null)
        {
            throw new Exception("Email already exists.");
        }

        // Create user
        var user = new Domain.Models.User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Role = Domain.Enums.UserRole.User
        };

        await _unitOfWork.userRepository.AddAsync(user);
        await _unitOfWork.SaveAsync();

        // Generate JWT token
        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse(token, request.Username, request.Email);
    }
}
