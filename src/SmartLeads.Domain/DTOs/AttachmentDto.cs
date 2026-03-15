namespace SmartLeads.Domain.DTOs;

public record AttachmentDto(
    int Id,
    string FileName,
    string FilePath,
    long FileSize,
    string FileType,
    int ContactId,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
