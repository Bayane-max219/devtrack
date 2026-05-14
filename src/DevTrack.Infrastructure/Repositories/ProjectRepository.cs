using DevTrack.Domain.Entities;
using DevTrack.Domain.Interfaces;
using DevTrack.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DevTrack.Infrastructure.Repositories;

public class ProjectRepository(AppDbContext db) : IProjectRepository
{
    public async Task<Project?> GetByIdAsync(Guid id) =>
        await db.Projects
            .Include(p => p.Members).ThenInclude(m => m.User)
            .Include(p => p.Tickets)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Project>> GetAllAsync() =>
        await db.Projects.Include(p => p.Members).ToListAsync();

    public async Task<IEnumerable<Project>> GetByUserIdAsync(Guid userId) =>
        await db.Projects
            .Include(p => p.Members)
            .Where(p => p.Members.Any(m => m.UserId == userId))
            .ToListAsync();

    public async Task AddAsync(Project project)
    {
        await db.Projects.AddAsync(project);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        db.Projects.Update(project);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var project = await db.Projects.FindAsync(id);
        if (project != null)
        {
            db.Projects.Remove(project);
            await db.SaveChangesAsync();
        }
    }
}
