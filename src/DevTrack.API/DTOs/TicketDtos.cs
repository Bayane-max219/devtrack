using DevTrack.Domain.Enums;

namespace DevTrack.API.DTOs;

public record CreateTicketRequest(
    string Title,
    string Description,
    TicketPriority Priority,
    Guid ProjectId,
    Guid? AssigneeId,
    DateTime? DueDate
);

public record UpdateTicketRequest(
    string Title,
    string Description,
    TicketPriority Priority,
    Guid? AssigneeId,
    DateTime? DueDate
);

public record UpdateTicketStatusRequest(TicketStatus Status);

public record TicketAssigneeDto(Guid Id, string FullName, string Email);

public record TicketDto(
    Guid Id,
    string Title,
    string Description,
    TicketStatus Status,
    TicketPriority Priority,
    Guid ProjectId,
    string ProjectName,
    TicketAssigneeDto? Assignee,
    DateTime CreatedAt,
    DateTime? DueDate,
    int CommentCount
);

public record TicketSummaryDto(
    Guid Id,
    string Title,
    TicketStatus Status,
    TicketPriority Priority,
    TicketAssigneeDto? Assignee,
    DateTime? DueDate
);

public record DashboardStatsDto(
    int TotalTickets,
    int Backlog,
    int InProgress,
    int InReview,
    int Done,
    int MyOpenTickets,
    IEnumerable<ProjectTicketCountDto> ByProject
);

public record ProjectTicketCountDto(Guid ProjectId, string ProjectName, int Count);
