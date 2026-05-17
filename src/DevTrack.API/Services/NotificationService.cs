using DevTrack.API.DTOs;
using DevTrack.API.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DevTrack.API.Services;

public class NotificationService(IHubContext<NotificationHub> hub)
{
    public async Task NotifyTicketAssigned(Guid assigneeId, TicketAssignedNotification notification)
    {
        await hub.Clients.Group($"user_{assigneeId}")
            .SendAsync("TicketAssigned", notification);

        await hub.Clients.Group($"project_{notification.ProjectId}")
            .SendAsync("ProjectTicketAssigned", notification);
    }

    public async Task NotifyTicketStatusChanged(Guid projectId, TicketStatusChangedNotification notification)
    {
        await hub.Clients.Group($"project_{projectId}")
            .SendAsync("TicketStatusChanged", notification);
    }

    public async Task NotifyCommentAdded(Guid ticketAssigneeId, TicketCommentAddedNotification notification)
    {
        if (ticketAssigneeId != Guid.Empty)
        {
            await hub.Clients.Group($"user_{ticketAssigneeId}")
                .SendAsync("CommentAdded", notification);
        }

        await hub.Clients.Group($"project_{notification.ProjectId}")
            .SendAsync("ProjectCommentAdded", notification);
    }
}
