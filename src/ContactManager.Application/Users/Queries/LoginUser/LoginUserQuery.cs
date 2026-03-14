using MediatR;
using ContactManager.Application.Common.Interfaces;
using ContactManager.Application.Common.Models;
using ContactManager.Domain.Interfaces.Repositories;
using ContactManager.Domain.Models;

namespace ContactManager.Application.Users.Queries.LoginUser;

public record LoginUserQuery(string EmailOrUsername, string Password) : IRequest<AuthResponse>;

public class LoginUserQueryHandler : IRequestHandler<LoginUserQuery, AuthResponse>
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginUserQueryHandler(
        IGenericRepository<User> userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<AuthResponse> Handle(LoginUserQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.FindAsync(u => u.Email == request.EmailOrUsername || u.Username == request.EmailOrUsername);
        var user = users.FirstOrDefault();

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new Exception("Invalid credentials.");
        }

        var token = _jwtTokenGenerator.GenerateToken(user);

        return new AuthResponse(token, user.Username, user.Email);
    }
}
