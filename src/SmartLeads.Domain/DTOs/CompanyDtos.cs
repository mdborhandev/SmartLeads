namespace SmartLeads.Domain.DTOs;

public record CompanyDto(
    Guid Id,
    string Name,
    string? Code,
    string? Address,
    string? Phone,
    string? Email,
    string? Logo,
    bool IsActive,
    bool IsParent,
    Guid? ParentCompanyId,
    string? ParentCompanyName,
    DateTime CreatedAt
);

public record CreateCompanyRequest(
    string Name,
    string? Code,
    string? Address,
    string? Phone,
    string? Email,
    string? Logo,
    bool IsParent,
    Guid? ParentCompanyId
);

public record UpdateCompanyRequest(
    Guid Id,
    string Name,
    string? Code,
    string? Address,
    string? Phone,
    string? Email,
    string? Logo,
    bool IsActive,
    bool IsParent,
    Guid? ParentCompanyId
);
