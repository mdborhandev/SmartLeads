namespace SmartLeads.Domain.DTOs;

public record UserDto(
    int Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
