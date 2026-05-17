using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DevTrack.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

        await base.OnConnectedAsync();
    }

    public async Task JoinProject(string projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
    }

    public async Task LeaveProject(string projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project_{projectId}");
    }
}
