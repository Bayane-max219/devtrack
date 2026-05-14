using DevTrack.Domain.Entities;
using DevTrack.Domain.Enums;
using DevTrack.Domain.Interfaces;
using DevTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevTrack.Infrastructure.Repositories;

public class TicketRepository(AppDbContext db) : ITicketRepository
{
    public async Task<Ticket?> GetByIdAsync(Guid id) =>
        await db.Tickets
            .Include(t => t.Assignee)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<IEnumerable<Ticket>> GetByProjectIdAsync(Guid projectId) =>
        await db.Tickets
            .Include(t => t.Assignee)
            .Where(t => t.ProjectId == projectId)
            .OrderBy(t => t.Status)
            .ToListAsync();

    public async Task<IEnumerable<Ticket>> GetByAssigneeIdAsync(Guid userId) =>
        await db.Tickets
            .Include(t => t.Project)
            .Where(t => t.AssigneeId == userId)
            .ToListAsync();

    public async Task<IEnumerable<Ticket>> GetByStatusAsync(Guid projectId, TicketStatus status) =>
        await db.Tickets
            .Where(t => t.ProjectId == projectId && t.Status == status)
            .ToListAsync();

    public async Task AddAsync(Ticket ticket)
    {
        await db.Tickets.AddAsync(ticket);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Ticket ticket)
    {
        db.Tickets.Update(ticket);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var ticket = await db.Tickets.FindAsync(id);
        if (ticket != null)
        {
            db.Tickets.Remove(ticket);
            await db.SaveChangesAsync();
        }
    }
}
