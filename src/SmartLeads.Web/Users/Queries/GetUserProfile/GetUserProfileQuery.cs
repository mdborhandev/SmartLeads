using MediatR;
using SmartLeads.Domain.Models;
using SmartLeads.Infrastructure.Repositories.Interface;

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
    private readonly IUserRepository _userRepository;

    public GetUserProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileDto> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);

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
