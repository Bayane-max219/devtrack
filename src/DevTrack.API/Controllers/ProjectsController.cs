using System.Security.Claims;
using DevTrack.API.DTOs;
using DevTrack.Domain.Entities;
using DevTrack.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController(IProjectRepository projectRepo, IUserRepository userRepo) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetMyProjects()
    {
        var projects = await projectRepo.GetByUserIdAsync(CurrentUserId);
        var result = projects.Select(p => new ProjectSummaryDto(
            p.Id, p.Name, p.CreatedAt, p.Deadline,
            p.Tickets.Count, p.Members.Count
        ));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProject(Guid id)
    {
        var project = await projectRepo.GetByIdAsync(id);
        if (project is null) return NotFound();

        var isMember = project.Members.Any(m => m.UserId == CurrentUserId);
        if (!isMember && !User.IsInRole("Admin")) return Forbid();

        var dto = MapToDto(project);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest req)
    {
        var project = new Project
        {
            Name = req.Name,
            Description = req.Description,
            Deadline = req.Deadline
        };

        project.Members.Add(new ProjectMember
        {
            UserId = CurrentUserId,
            Role = "Owner"
        });

        await projectRepo.AddAsync(project);

        var created = await projectRepo.GetByIdAsync(project.Id);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, MapToDto(created!));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest req)
    {
        var project = await projectRepo.GetByIdAsync(id);
        if (project is null) return NotFound();

        var member = project.Members.FirstOrDefault(m => m.UserId == CurrentUserId);
        if (member is null || (member.Role != "Owner" && member.Role != "Manager"))
            return Forbid();

        project.Name = req.Name;
        project.Description = req.Description;
        project.Deadline = req.Deadline;
        await projectRepo.UpdateAsync(project);

        return Ok(MapToDto(project));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var project = await projectRepo.GetByIdAsync(id);
        if (project is null) return NotFound();

        var member = project.Members.FirstOrDefault(m => m.UserId == CurrentUserId);
        if (member?.Role != "Owner" && !User.IsInRole("Admin")) return Forbid();

        await projectRepo.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:guid}/members")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddMemberRequest req)
    {
        var project = await projectRepo.GetByIdAsync(id);
        if (project is null) return NotFound();

        var requester = project.Members.FirstOrDefault(m => m.UserId == CurrentUserId);
        if (requester is null || (requester.Role != "Owner" && requester.Role != "Manager"))
            return Forbid();

        if (project.Members.Any(m => m.UserId == req.UserId))
            return Conflict(new { message = "User is already a member of this project" });

        var user = await userRepo.GetByIdAsync(req.UserId);
        if (user is null) return NotFound(new { message = "User not found" });

        project.Members.Add(new ProjectMember { UserId = req.UserId, Role = req.Role });
        await projectRepo.UpdateAsync(project);

        return Ok(new { message = "Member added successfully" });
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid id, Guid userId)
    {
        var project = await projectRepo.GetByIdAsync(id);
        if (project is null) return NotFound();

        var requester = project.Members.FirstOrDefault(m => m.UserId == CurrentUserId);
        if (requester?.Role != "Owner" && !User.IsInRole("Admin")) return Forbid();

        var member = project.Members.FirstOrDefault(m => m.UserId == userId);
        if (member is null) return NotFound(new { message = "Member not found" });

        project.Members.Remove(member);
        await projectRepo.UpdateAsync(project);

        return NoContent();
    }

    private static ProjectDto MapToDto(Project p) => new(
        p.Id, p.Name, p.Description, p.CreatedAt, p.Deadline,
        p.Tickets.Count, p.Members.Count,
        p.Members.Select(m => new ProjectMemberDto(
            m.UserId,
            m.User?.FirstName + " " + m.User?.LastName,
            m.User?.Email ?? "",
            m.Role
        ))
    );
}
