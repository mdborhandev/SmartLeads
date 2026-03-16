namespace SmartLeads.Domain.DTOs;

public record ContactDto(
    Guid Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Company,
    string? JobTitle,
    string? Address,
    bool IsArchived,
    Guid CompanyId = default,
    Guid UserId = default);
