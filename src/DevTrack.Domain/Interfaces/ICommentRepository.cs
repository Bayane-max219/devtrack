using DevTrack.Domain.Entities;

namespace DevTrack.Domain.Interfaces;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(Guid id);
    Task<IEnumerable<Comment>> GetByTicketIdAsync(Guid ticketId);
    Task AddAsync(Comment comment);
    Task DeleteAsync(Guid id);
}
