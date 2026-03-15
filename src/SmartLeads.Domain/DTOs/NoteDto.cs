namespace SmartLeads.Domain.DTOs;

public record NoteDto(
    int Id,
    string Title,
    string Content,
    int ContactId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
