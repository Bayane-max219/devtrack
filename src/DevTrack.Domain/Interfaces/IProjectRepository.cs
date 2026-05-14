using DevTrack.Domain.Entities;

namespace DevTrack.Domain.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id);
    Task<IEnumerable<Project>> GetAllAsync();
    Task<IEnumerable<Project>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Guid id);
}
