using Microsoft.AspNetCore.SignalR;

namespace HawkeyeServer.Api.Endpoints;

public class TripHub : Hub
{
    public async Task Ping(string message)
    {
        await Clients.Caller.SendAsync("Pong", $"Server-{message}");
    }

    public override async Task OnConnectedAsync()
    {
        var tripId = Context.GetHttpContext()?.Request.Query["tripId"];
        if (!string.IsNullOrEmpty(tripId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"trip:{tripId}");
        }
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var tripId = Context.GetHttpContext()?.Request.Query["tripId"];
        if (!string.IsNullOrEmpty(tripId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"trip:{tripId}");
            await Clients
                .Group($"trip:{tripId}")
                .SendAsync("UserDisconnected", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}
