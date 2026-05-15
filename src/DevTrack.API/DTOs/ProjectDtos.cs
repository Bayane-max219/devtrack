namespace DevTrack.API.DTOs;

public record CreateProjectRequest(
    string Name,
    string Description,
    DateTime? Deadline
);

public record UpdateProjectRequest(
    string Name,
    string Description,
    DateTime? Deadline
);

public record AddMemberRequest(Guid UserId, string Role);

public record ProjectMemberDto(
    Guid UserId,
    string FullName,
    string Email,
    string Role
);

public record ProjectDto(
    Guid Id,
    string Name,
    string Description,
    DateTime CreatedAt,
    DateTime? Deadline,
    int TicketCount,
    int MemberCount,
    IEnumerable<ProjectMemberDto> Members
);

public record ProjectSummaryDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    DateTime? Deadline,
    int TicketCount,
    int MemberCount
);
