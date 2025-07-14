namespace HawkeyeServer.Api.Models;

public record Trip
{
    public long Id { get; init; }
    public required long CreatedBy { get; set; }
    public required string Title { get; set; }
    public required string Destination { get; set; }
    public required DateOnly StartDate { get; set; } // Editable
    public required DateOnly EndDate { get; set; } // Editable
    public required string JoinCode { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record Place
{
    public long Id { get; init; }
    public required double Longitude { get; init; }
    public required double Latitude { get; init; }
    public required string Name { get; set; }
    public required DateOnly Date { get; set; } // Editable
}

public record Activity
{
    public long Id { get; init; }
    public required string Name { get; set; } // Editable
    public required TimeOnly StartTime { get; set; } // Editable
}

public record TripMate
{
    public required long TripId { get; init; }
    public required long UserId { get; init; }
    public DateTime AddedAt { get; init; }
}

public record WithPlaces<T>(T Trip)
    where T : Trip
{
    public List<WithActivities<Place>> Places { get; set; } = []; // Add/Remove
}

public record WithActivities<T>(T Place)
    where T : Place
{
    public List<Activity> Activities { get; set; } = []; // Add/Remove
}
