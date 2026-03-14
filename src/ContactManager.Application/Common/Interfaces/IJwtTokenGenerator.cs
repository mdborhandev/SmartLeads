using ContactManager.Domain.Models;

namespace ContactManager.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
