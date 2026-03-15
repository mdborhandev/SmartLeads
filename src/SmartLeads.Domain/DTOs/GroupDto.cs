namespace SmartLeads.Domain.DTOs;

public record GroupDto(
    int Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
