using MediatR;
using SmartLeads.Domain.Interfaces.Repositories;
using SmartLeads.Domain.Models;

namespace SmartLeads.Web.Users.Queries.GetUserProfile;

public record GetUserProfileQuery(string UsernameOrEmail) : IRequest<UserProfileDto>;

public record UserProfileDto(
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class GetUserProfileQueryHandler : IRequestHandler<GetUserProfileQuery, UserProfileDto>
{
    private readonly IGenericRepository<User> _userRepository;

    public GetUserProfileQueryHandler(IGenericRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.FindAsync(u => u.Username == request.UsernameOrEmail || u.Email == request.UsernameOrEmail);
        var user = users.FirstOrDefault();

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        return new UserProfileDto(
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
