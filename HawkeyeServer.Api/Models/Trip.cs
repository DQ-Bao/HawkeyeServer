namespace HawkeyeServer.Api.Models;

public record Trip
{
    public long Id { get; init; }
    public required string Title { get; set; }
    public required string Destination { get; set; }
    public required DateOnly StartDate { get; set; }
    public required DateOnly EndDate { get; set; }
    public DateTime CreatedAt { get; init; }
}
