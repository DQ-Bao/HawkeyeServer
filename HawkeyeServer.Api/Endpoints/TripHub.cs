using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HawkeyeServer.Api.Data;
using HawkeyeServer.Api.Models;
using Microsoft.AspNetCore.SignalR;

namespace HawkeyeServer.Api.Endpoints;

public class TripHub(ITripDataAccess trips, TripMemoryStore memory) : Hub
{
    public async Task Ping(string message)
    {
        await Clients.Caller.SendAsync("Pong", $"Server-{message}");
    }

    public async Task UpdateTripDates(DateOnly startDate, DateOnly endDate)
    {
        if (!Context.Items.TryGetValue("tripId", out var tripIdObj) || tripIdObj is not long tripId)
        {
            Context.Abort();
            return;
        }
        if (!Context.Items.TryGetValue("user", out var userObj) || userObj is not User user)
        {
            Context.Abort();
            return;
        }

        memory.UpdateTrip(
            tripId,
            withPlaces =>
            {
                withPlaces.Trip.StartDate = startDate;
                withPlaces.Trip.EndDate = endDate;
            }
        );

        await Clients
            .OthersInGroup($"trip:{tripId}")
            .SendAsync("ReceiveTripDates", startDate, endDate, user.Name);
    }

    public async Task UpdatePlaceDate(long placeId, DateOnly date)
    {
        if (!Context.Items.TryGetValue("tripId", out var tripIdObj) || tripIdObj is not long tripId)
        {
            Context.Abort();
            return;
        }
        if (!Context.Items.TryGetValue("user", out var userObj) || userObj is not User user)
        {
            Context.Abort();
            return;
        }

        memory.UpdateTrip(
            tripId,
            withPlaces =>
            {
                if (date >= withPlaces.Trip.StartDate && date <= withPlaces.Trip.EndDate)
                {
                    var index = withPlaces.Places.FindIndex(p => p.Place.Id == placeId);
                    if (index != -1)
                    {
                        withPlaces.Places[index].Place.Date = date;
                    }
                }
            }
        );

        await Clients
            .OthersInGroup($"trip:{tripId}")
            .SendAsync("ReceivePlaceDate", date, user.Name);
    }

    public async Task UpdateActivity(long actId, string name, TimeOnly startTime)
    {
        if (!Context.Items.TryGetValue("tripId", out var tripIdObj) || tripIdObj is not long tripId)
        {
            Context.Abort();
            return;
        }
        if (!Context.Items.TryGetValue("user", out var userObj) || userObj is not User user)
        {
            Context.Abort();
            return;
        }

        memory.UpdateTrip(
            tripId,
            withPlaces =>
            {
                for (int i = 0; i < withPlaces.Places.Count; i++)
                {
                    var index = withPlaces.Places[i].Activities.FindIndex(a => a.Id == actId);
                    if (index != -1)
                    {
                        withPlaces.Places[i].Activities[index].Name = name;
                        withPlaces.Places[i].Activities[index].StartTime = startTime;
                    }
                }
            }
        );

        await Clients
            .OthersInGroup($"trip:{tripId}")
            .SendAsync("ReceiveActivity", name, startTime, user.Name);
    }

    public async Task AddPlace(double longitude, double latitude, string name, DateOnly date)
    {
        if (!Context.Items.TryGetValue("tripId", out var tripIdObj) || tripIdObj is not long tripId)
        {
            Context.Abort();
            return;
        }
        if (!Context.Items.TryGetValue("user", out var userObj) || userObj is not User user)
        {
            Context.Abort();
            return;
        }
        memory.UpdateTrip(
            tripId,
            withPlaces =>
                withPlaces.Places.Add(
                    new WithActivities<Place>(
                        new Place
                        {
                            Longitude = longitude,
                            Latitude = latitude,
                            Name = name,
                            Date = date,
                        }
                    )
                )
        );
        await Clients
            .OthersInGroup($"trip:{tripId}")
            .SendAsync("ReceiveAddedPlace", longitude, latitude, name, date, user.Name);
    }

    public async Task RemovePlace(long placeId)
    {
        if (!Context.Items.TryGetValue("tripId", out var tripIdObj) || tripIdObj is not long tripId)
        {
            Context.Abort();
            return;
        }
        if (!Context.Items.TryGetValue("user", out var userObj) || userObj is not User user)
        {
            Context.Abort();
            return;
        }

        memory.UpdateTrip(
            tripId,
            withPlaces =>
            {
                var index = withPlaces.Places.FindIndex(p => p.Place.Id == placeId);
                if (index != -1)
                {
                    withPlaces.Places.RemoveAt(index);
                }
            }
        );

        await Clients
            .OthersInGroup($"trip:{tripId}")
            .SendAsync("ReceiveRemovedPlace", placeId, user.Name);
    }

    public async Task AddActivity(long placeId, string name, TimeOnly startTime)
    {
        if (!Context.Items.TryGetValue("tripId", out var tripIdObj) || tripIdObj is not long tripId)
        {
            Context.Abort();
            return;
        }
        if (!Context.Items.TryGetValue("user", out var userObj) || userObj is not User user)
        {
            Context.Abort();
            return;
        }

        memory.UpdateTrip(
            tripId,
            withPlaces =>
            {
                var index = withPlaces.Places.FindIndex(p => p.Place.Id == placeId);
                if (index != -1)
                {
                    withPlaces
                        .Places[index]
                        .Activities.Add(new Activity { Name = name, StartTime = startTime });
                }
            }
        );

        await Clients
            .OthersInGroup($"trip:{tripId}")
            .SendAsync("ReceiveAddedActivity", name, startTime, user.Name);
    }

    public async Task RemoveActivity(long actId)
    {
        if (!Context.Items.TryGetValue("tripId", out var tripIdObj) || tripIdObj is not long tripId)
        {
            Context.Abort();
            return;
        }
        if (!Context.Items.TryGetValue("user", out var userObj) || userObj is not User user)
        {
            Context.Abort();
            return;
        }

        memory.UpdateTrip(
            tripId,
            withPlaces =>
            {
                for (int i = 0; i < withPlaces.Places.Count; i++)
                {
                    var index = withPlaces.Places[i].Activities.FindIndex(a => a.Id == actId);
                    if (index != -1)
                    {
                        withPlaces.Places[i].Activities.RemoveAt(index);
                    }
                }
            }
        );

        await Clients
            .OthersInGroup($"trip:{tripId}")
            .SendAsync("ReceiveRemovedActivity", actId, user.Name);
    }

    public async Task SendLocation(object location)
    {
        if (!Context.Items.TryGetValue("tripId", out var tripIdObj) || tripIdObj is not long tripId)
        {
            Context.Abort();
            return;
        }
        await Clients.OthersInGroup($"trip:{tripId}").SendAsync("ReceiveLocation", location);
    }

    public override async Task OnConnectedAsync()
    {
        var tripIdStr = Context.GetHttpContext()?.Request.Query["tripId"];
        if (string.IsNullOrEmpty(tripIdStr) || !long.TryParse(tripIdStr, out var tripId))
        {
            Context.Abort();
            return;
        }

        var userIdStr = Context.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!long.TryParse(userIdStr, out var userId))
        {
            Context.Abort();
            return;
        }

        var user = await trips.GetMateAsync(tripId, userId);
        if (user is null)
        {
            Context.Abort();
            return;
        }

        var trip = memory.GetTrip(tripId);
        if (trip is null)
        {
            trip = await trips.GetByIdAsync(tripId);
            if (trip is null)
            {
                Context.Abort();
                return;
            }
            memory.SetTrip(tripId, trip);
        }

        Context.Items["tripId"] = tripId;
        Context.Items["user"] = user;

        await Groups.AddToGroupAsync(Context.ConnectionId, $"trip:{tripId}");
        await Task.WhenAll(
            Clients.Caller.SendAsync("TripConnected", trip),
            Clients
                .OthersInGroup($"trip:{tripId}")
                .SendAsync(
                    "UserConnected",
                    new
                    {
                        user.Id,
                        user.Name,
                        user.Picture,
                    }
                )
        );
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (!Context.Items.TryGetValue("tripId", out var tripIdObj) || tripIdObj is not long tripId)
        {
            Context.Abort();
            return;
        }
        if (!Context.Items.TryGetValue("user", out var userObj) || userObj is not User user)
        {
            Context.Abort();
            return;
        }
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"trip:{tripId}");
        await Task.WhenAll(
            Clients.Caller.SendAsync("TripDisconnected"),
            Clients.Group($"trip:{tripId}").SendAsync("UserDisconnected", user.Id)
        );
        await base.OnDisconnectedAsync(exception);
    }
}
