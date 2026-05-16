using DevTrack.Domain.Entities;
using DevTrack.Domain.Interfaces;
using DevTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevTrack.Infrastructure.Repositories;

public class CommentRepository(AppDbContext db) : ICommentRepository
{
    public async Task<Comment?> GetByIdAsync(Guid id) =>
        await db.Comments
            .Include(c => c.Author)
            .FirstOrDefaultAsync(c => c.Id == id);

    public async Task<IEnumerable<Comment>> GetByTicketIdAsync(Guid ticketId) =>
        await db.Comments
            .Include(c => c.Author)
            .Where(c => c.TicketId == ticketId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

    public async Task AddAsync(Comment comment)
    {
        await db.Comments.AddAsync(comment);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var comment = await db.Comments.FindAsync(id);
        if (comment != null)
        {
            db.Comments.Remove(comment);
            await db.SaveChangesAsync();
        }
    }
}
