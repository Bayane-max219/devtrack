using System.Security.Claims;
using DevTrack.API.DTOs;
using DevTrack.API.Services;
using DevTrack.Domain.Entities;
using DevTrack.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CommentsController(
    ICommentRepository commentRepo,
    ITicketRepository ticketRepo,
    NotificationService notificationService) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private string CurrentUserName =>
        User.FindFirstValue(ClaimTypes.Name) ?? "Unknown";

    [HttpGet("ticket/{ticketId:guid}")]
    public async Task<IActionResult> GetByTicket(Guid ticketId)
    {
        var ticket = await ticketRepo.GetByIdAsync(ticketId);
        if (ticket is null) return NotFound();

        var comments = await commentRepo.GetByTicketIdAsync(ticketId);
        var result = comments.Select(MapToDto);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddComment([FromBody] CreateCommentRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Content))
            return BadRequest(new { message = "Content cannot be empty" });

        var ticket = await ticketRepo.GetByIdAsync(req.TicketId);
        if (ticket is null) return NotFound(new { message = "Ticket not found" });

        var comment = new Comment
        {
            Content = req.Content,
            TicketId = req.TicketId,
            AuthorId = CurrentUserId
        };

        await commentRepo.AddAsync(comment);
        var created = await commentRepo.GetByIdAsync(comment.Id);

        var preview = req.Content.Length > 80 ? req.Content[..80] + "..." : req.Content;
        var assigneeId = ticket.AssigneeId ?? Guid.Empty;
        await notificationService.NotifyCommentAdded(assigneeId, new TicketCommentAddedNotification(
            ticket.Id, ticket.Title, ticket.ProjectId,
            CurrentUserName, preview, DateTime.UtcNow));

        return CreatedAtAction(nameof(GetByTicket), new { ticketId = req.TicketId }, MapToDto(created!));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteComment(Guid id)
    {
        var comment = await commentRepo.GetByIdAsync(id);
        if (comment is null) return NotFound();

        if (comment.AuthorId != CurrentUserId)
            return Forbid();

        await commentRepo.DeleteAsync(id);
        return NoContent();
    }

    private static CommentDto MapToDto(Comment c) => new(
        c.Id,
        c.Content,
        c.CreatedAt,
        new CommentAuthorDto(c.Author.Id, c.Author.FirstName + " " + c.Author.LastName),
        c.TicketId
    );
}
