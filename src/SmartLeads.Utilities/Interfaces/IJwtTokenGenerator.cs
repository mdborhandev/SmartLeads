using SmartLeads.Domain.Models;

namespace SmartLeads.Utilities.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
