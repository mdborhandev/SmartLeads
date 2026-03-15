using SmartLeads.Domain.Models;

namespace SmartLeads.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
