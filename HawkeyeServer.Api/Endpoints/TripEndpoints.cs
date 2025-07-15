using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HawkeyeServer.Api.Data;
using HawkeyeServer.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace HawkeyeServer.Api.Endpoints;

public record CreateTripRequest(
    [Required] string Destination,
    [Required] DateOnly StartDate,
    [Required] DateOnly EndDate
) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate >= EndDate)
        {
            yield return new ValidationResult(
                "Start date must be before end date",
                new[] { nameof(StartDate), nameof(EndDate) }
            );
        }
    }
};

public static class TripEndpoints
{
    public static IEndpointRouteBuilder MapTripEndpoints(this IEndpointRouteBuilder app)
    {
        var routes = app.MapGroup("/api/v1/trips");
        routes
            .MapGet(
                "/me",
                async (ClaimsPrincipal user, ITripDataAccess trips) =>
                    Results.Ok(
                        await trips.GetAllByUser(
                            long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!)
                        )
                    )
            )
            .RequireAuthorization();
        routes
            .MapPost(
                "",
                async (
                    [FromBody] CreateTripRequest req,
                    ClaimsPrincipal user,
                    ITripDataAccess trips
                ) =>
                {
                    var userId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    var trip = await trips.AddAsync(
                        new Trip
                        {
                            CreatedBy = userId,
                            Title = $"Trip to {req.Destination}",
                            Destination = req.Destination,
                            StartDate = req.StartDate,
                            EndDate = req.EndDate,
                            JoinCode = GenerateJoinCode(),
                        }
                    );
                    return Results.Created($"/trips/{trip.Id}", trip);
                }
            )
            .AddEndpointFilter<ValidationFilter<CreateTripRequest>>()
            .RequireAuthorization();
        routes
            .MapPost(
                "/join/{code}",
                async ([FromRoute] string code, ClaimsPrincipal user, ITripDataAccess trips) =>
                {
                    var userId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
                    var withPlaces = await trips.GetByCodeAsync(code);
                    if (withPlaces is null)
                        return Results.NotFound("Trip not found");
                    var mate = await trips.AddMateAsync(
                        new TripMate { UserId = userId, TripId = withPlaces.Trip.Id }
                    );
                    return Results.Created($"/trips/{withPlaces.Trip.Id}", mate);
                }
            )
            .RequireAuthorization();
        return app;
    }

    private static string GenerateJoinCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(
            Enumerable.Range(0, 10).Select(_ => chars[random.Next(chars.Length)]).ToArray()
        );
    }
}
