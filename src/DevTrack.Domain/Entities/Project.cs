namespace DevTrack.Domain.Entities;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? Deadline { get; set; }

    public ICollection<ProjectMember> Members { get; set; } = [];
    public ICollection<Ticket> Tickets { get; set; } = [];
}
