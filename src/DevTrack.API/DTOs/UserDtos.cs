namespace DevTrack.API.DTOs;

public record UpdateProfileRequest(string FirstName, string LastName);

public record UserProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    DateTime CreatedAt
);

public record UserSummaryDto(Guid Id, string FullName, string Email);
