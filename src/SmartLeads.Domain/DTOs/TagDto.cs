namespace SmartLeads.Domain.DTOs;

public record TagDto(
    int Id,
    string Name,
    string? Color,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
