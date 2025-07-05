namespace HawkeyeServer.Api.Models;

public record User
{
    public long Id { get; init; }
    public required string Email { get; set; }
    public required string Name { get; set; }
    public string? Picture { get; set; }
    public DateTime CreatedAt { get; init; }
}
