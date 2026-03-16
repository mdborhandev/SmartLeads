using SmartLeads.Domain.Models;

namespace SmartLeads.Domain.Interfaces.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
