using DevTrack.Domain.Entities;
using DevTrack.Domain.Enums;

namespace DevTrack.Domain.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id);
    Task<IEnumerable<Ticket>> GetByProjectIdAsync(Guid projectId);
    Task<IEnumerable<Ticket>> GetByAssigneeIdAsync(Guid userId);
    Task<IEnumerable<Ticket>> GetByStatusAsync(Guid projectId, TicketStatus status);
    Task AddAsync(Ticket ticket);
    Task UpdateAsync(Ticket ticket);
    Task DeleteAsync(Guid id);
}
