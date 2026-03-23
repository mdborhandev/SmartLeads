using MediatR;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Infrastructure.Services.Interface;
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
    private readonly IUserService _userService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public RegisterUserCommandHandler(
        IUnitOfWork unitOfWork,
        IUserService userService,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var (success, token, error) = await _userService.RegisterAsync(
            request.Username,
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName
        );

        if (!success || token == null)
        {
            throw new Exception(error ?? "Registration failed.");
        }

        return new AuthResponse(token, request.Username, request.Email);
    }
}
