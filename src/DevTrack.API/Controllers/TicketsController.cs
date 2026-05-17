using System.Security.Claims;
using DevTrack.API.DTOs;
using DevTrack.API.Services;
using DevTrack.Domain.Entities;
using DevTrack.Domain.Enums;
using DevTrack.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController(
    ITicketRepository ticketRepo,
    IProjectRepository projectRepo,
    NotificationService notificationService) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string CurrentUserName =>
        User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

    [HttpGet("project/{projectId:guid}")]
    public async Task<IActionResult> GetProjectTickets(Guid projectId, [FromQuery] TicketStatus? status)
    {
        var project = await projectRepo.GetByIdAsync(projectId);
        if (project is null) return NotFound();

        var tickets = status.HasValue
            ? await ticketRepo.GetByStatusAsync(projectId, status.Value)
            : await ticketRepo.GetByProjectIdAsync(projectId);

        var result = tickets.Select(MapToSummary);
        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTickets()
    {
        var tickets = await ticketRepo.GetByAssigneeIdAsync(CurrentUserId);
        var result = tickets.Select(MapToSummary);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTicket(Guid id)
    {
        var ticket = await ticketRepo.GetByIdAsync(id);
        if (ticket is null) return NotFound();
        return Ok(MapToDto(ticket));
    }

    [HttpPost]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequest req)
    {
        var project = await projectRepo.GetByIdAsync(req.ProjectId);
        if (project is null) return NotFound(new { message = "Project not found" });

        var isMember = project.Members.Any(m => m.UserId == CurrentUserId);
        if (!isMember) return Forbid();

        var ticket = new Ticket
        {
            Title = req.Title,
            Description = req.Description,
            Priority = req.Priority,
            ProjectId = req.ProjectId,
            AssigneeId = req.AssigneeId,
            DueDate = req.DueDate
        };

        await ticketRepo.AddAsync(ticket);
        var created = await ticketRepo.GetByIdAsync(ticket.Id);

        if (req.AssigneeId.HasValue && req.AssigneeId.Value != CurrentUserId)
        {
            await notificationService.NotifyTicketAssigned(req.AssigneeId.Value, new TicketAssignedNotification(
                ticket.Id, ticket.Title, req.ProjectId, project.Name,
                CurrentUserName, DateTime.UtcNow));
        }

        return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, MapToDto(created!));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTicket(Guid id, [FromBody] UpdateTicketRequest req)
    {
        var ticket = await ticketRepo.GetByIdAsync(id);
        if (ticket is null) return NotFound();

        var previousAssigneeId = ticket.AssigneeId;

        ticket.Title = req.Title;
        ticket.Description = req.Description;
        ticket.Priority = req.Priority;
        ticket.AssigneeId = req.AssigneeId;
        ticket.DueDate = req.DueDate;

        await ticketRepo.UpdateAsync(ticket);

        if (req.AssigneeId.HasValue && req.AssigneeId != previousAssigneeId && req.AssigneeId.Value != CurrentUserId)
        {
            var project = await projectRepo.GetByIdAsync(ticket.ProjectId);
            await notificationService.NotifyTicketAssigned(req.AssigneeId.Value, new TicketAssignedNotification(
                ticket.Id, ticket.Title, ticket.ProjectId, project?.Name ?? "",
                CurrentUserName, DateTime.UtcNow));
        }

        return Ok(MapToDto(ticket));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateTicketStatusRequest req)
    {
        var ticket = await ticketRepo.GetByIdAsync(id);
        if (ticket is null) return NotFound();

        var oldStatus = ticket.Status.ToString();
        ticket.Status = req.Status;
        await ticketRepo.UpdateAsync(ticket);

        await notificationService.NotifyTicketStatusChanged(ticket.ProjectId, new TicketStatusChangedNotification(
            ticket.Id, ticket.Title, ticket.ProjectId,
            oldStatus, req.Status.ToString(),
            CurrentUserName, DateTime.UtcNow));

        return Ok(new { id = ticket.Id, status = ticket.Status.ToString() });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTicket(Guid id)
    {
        var ticket = await ticketRepo.GetByIdAsync(id);
        if (ticket is null) return NotFound();

        await ticketRepo.DeleteAsync(id);
        return NoContent();
    }

    private static TicketSummaryDto MapToSummary(Ticket t) => new(
        t.Id, t.Title, t.Status, t.Priority,
        t.Assignee is null ? null : new TicketAssigneeDto(t.Assignee.Id, t.Assignee.FirstName + " " + t.Assignee.LastName, t.Assignee.Email),
        t.DueDate
    );

    private static TicketDto MapToDto(Ticket t) => new(
        t.Id, t.Title, t.Description, t.Status, t.Priority,
        t.ProjectId, t.Project?.Name ?? "",
        t.Assignee is null ? null : new TicketAssigneeDto(t.Assignee.Id, t.Assignee.FirstName + " " + t.Assignee.LastName, t.Assignee.Email),
        t.CreatedAt, t.DueDate, t.Comments.Count
    );
}
