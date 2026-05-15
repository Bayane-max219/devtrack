using System.Security.Claims;
using DevTrack.API.DTOs;
using DevTrack.Domain.Enums;
using DevTrack.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(ITicketRepository ticketRepo, IProjectRepository projectRepo) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var myProjects = await projectRepo.GetByUserIdAsync(CurrentUserId);
        var myProjectIds = myProjects.Select(p => p.Id).ToList();

        var allTickets = new List<Domain.Entities.Ticket>();
        foreach (var pid in myProjectIds)
        {
            var tickets = await ticketRepo.GetByProjectIdAsync(pid);
            allTickets.AddRange(tickets);
        }

        var myOpenTickets = await ticketRepo.GetByAssigneeIdAsync(CurrentUserId);

        var byProject = myProjects.Select(p => new ProjectTicketCountDto(
            p.Id,
            p.Name,
            allTickets.Count(t => t.ProjectId == p.Id)
        ));

        var stats = new DashboardStatsDto(
            TotalTickets: allTickets.Count,
            Backlog: allTickets.Count(t => t.Status == TicketStatus.Backlog),
            InProgress: allTickets.Count(t => t.Status == TicketStatus.InProgress),
            InReview: allTickets.Count(t => t.Status == TicketStatus.Review),
            Done: allTickets.Count(t => t.Status == TicketStatus.Done),
            MyOpenTickets: myOpenTickets.Count(t => t.Status != TicketStatus.Done),
            ByProject: byProject
        );

        return Ok(stats);
    }
}
