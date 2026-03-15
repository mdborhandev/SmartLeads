namespace SmartLeads.Domain.DTOs;

public record ContactDto(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Company,
    string? JobTitle,
    string? Address,
    bool IsArchived,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
