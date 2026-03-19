namespace SmartLeads.Domain.DTOs;

public class UserTableDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? EmployeeId { get; set; }
    public string? Department { get; set; }
    public string? Designation { get; set; }
    public Domain.Enums.UserRole Role { get; set; }
    public string RoleName => GetRoleDisplayName(Role);
    public string RoleBadge => GetRoleBadgeClass(Role);
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }

    private string GetRoleBadgeClass(Domain.Enums.UserRole role)
    {
        return role switch
        {
            Domain.Enums.UserRole.SuperAdmin => "danger",
            Domain.Enums.UserRole.Admin => "primary",
            Domain.Enums.UserRole.Manager => "info",
            _ => "secondary"
        };
    }

    private string GetRoleDisplayName(Domain.Enums.UserRole role)
    {
        return role switch
        {
            Domain.Enums.UserRole.SuperAdmin => "Super Admin",
            Domain.Enums.UserRole.Admin => "Admin",
            Domain.Enums.UserRole.Manager => "Manager",
            _ => "User"
        };
    }
}
