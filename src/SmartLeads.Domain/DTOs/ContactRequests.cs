namespace SmartLeads.Domain.DTOs;

public record CreateContactRequest(
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Company,
    string? JobTitle,
    string? Address
);

public record UpdateContactRequest(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Company,
    string? JobTitle,
    string? Address
);
