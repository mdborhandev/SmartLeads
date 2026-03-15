namespace SmartLeads.Application.Contacts.Models;

public record ContactDto(
    int Id,
    string FirstName,
    string LastName,
    string? Email,
    string? PhoneNumber,
    string? Company,
    string? JobTitle,
    string? Address,
    bool IsArchived);
