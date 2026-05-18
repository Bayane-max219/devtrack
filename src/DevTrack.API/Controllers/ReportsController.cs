using System.Security.Claims;
using DevTrack.API.Services;
using DevTrack.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController(
    IProjectRepository projectRepo,
    ITicketRepository ticketRepo,
    SprintReportService reportService) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("sprint/{projectId:guid}")]
    public async Task<IActionResult> GetSprintReport(Guid projectId)
    {
        var project = await projectRepo.GetByIdAsync(projectId);
        if (project is null) return NotFound(new { message = "Project not found" });

        var isMember = project.Members.Any(m => m.UserId == CurrentUserId);
        if (!isMember) return Forbid();

        var tickets = await ticketRepo.GetByProjectIdAsync(projectId);
        var pdf = reportService.GenerateSprintReport(project, tickets);

        var filename = $"sprint-report-{project.Name.Replace(" ", "-").ToLower()}-{DateTime.UtcNow:yyyyMMdd}.pdf";

        return File(pdf, "application/pdf", filename);
    }
}
