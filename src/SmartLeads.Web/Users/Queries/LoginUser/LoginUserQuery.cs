using MediatR;
using SmartLeads.Domain.DTOs;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;
using SmartLeads.Utilities.Interfaces;

namespace SmartLeads.Web.Users.Queries.LoginUser;

public record LoginUserQuery(string EmailOrUsername, string Password) : IRequest<AuthResponse>;

public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserQueryHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.EmailOrUsername);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new Exception("Invalid credentials.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse(token, user.Username, user.Email);
    }
}
