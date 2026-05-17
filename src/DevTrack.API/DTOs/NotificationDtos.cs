namespace DevTrack.API.DTOs;

public record TicketAssignedNotification(
    Guid TicketId,
    string TicketTitle,
    Guid ProjectId,
    string ProjectName,
    string AssignedByName,
    DateTime AssignedAt
);

public record TicketStatusChangedNotification(
    Guid TicketId,
    string TicketTitle,
    Guid ProjectId,
    string OldStatus,
    string NewStatus,
    string ChangedByName,
    DateTime ChangedAt
);

public record TicketCommentAddedNotification(
    Guid TicketId,
    string TicketTitle,
    Guid ProjectId,
    string CommentAuthorName,
    string CommentPreview,
    DateTime CommentedAt
);
