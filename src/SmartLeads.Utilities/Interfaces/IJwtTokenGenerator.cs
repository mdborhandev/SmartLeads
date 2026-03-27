using SmartLeads.Domain.Models;
using SmartLeads.Domain.Enums;

namespace SmartLeads.Utilities.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, UserRole? role = null);
}
