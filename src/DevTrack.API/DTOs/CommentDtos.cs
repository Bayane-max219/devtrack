namespace DevTrack.API.DTOs;

public record CreateCommentRequest(Guid TicketId, string Content);

public record CommentAuthorDto(Guid Id, string FullName);

public record CommentDto(
    Guid Id,
    string Content,
    DateTime CreatedAt,
    CommentAuthorDto Author,
    Guid TicketId
);
