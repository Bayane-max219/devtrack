using DevTrack.Domain.Enums;

namespace DevTrack.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Developer;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProjectMember> ProjectMembers { get; set; } = [];
    public ICollection<Ticket> AssignedTickets { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}
