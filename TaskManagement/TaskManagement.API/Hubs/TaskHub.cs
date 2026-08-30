using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TaskManagement.API.Hubs
{
    [Authorize]
    public class TaskHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"User:{userId}");
            }

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    "Admins");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var role = Context.User?.FindFirstValue(ClaimTypes.Role);

            if (!string.IsNullOrWhiteSpace(userId))
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    $"User:{userId}");
            }

            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                await Groups.RemoveFromGroupAsync(
                    Context.ConnectionId,
                    "Admins");
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}