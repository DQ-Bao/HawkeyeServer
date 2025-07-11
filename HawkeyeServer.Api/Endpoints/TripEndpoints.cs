using System.ComponentModel.DataAnnotations;
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
        app.MapPost(
                "/api/v1/trip",
                async ([FromBody] CreateTripRequest req, ITripDataAccess trips) =>
                {
                    var trip = await trips.AddAsync(
                        new Trip
                        {
                            Title = $"Trip to {req.Destination}",
                            Destination = req.Destination,
                            StartDate = req.StartDate,
                            EndDate = req.EndDate,
                        }
                    );
                    return Results.Created($"/api/v1/trips/{trip.Id}", trip);
                }
            )
            .AddEndpointFilter<ValidationFilter<CreateTripRequest>>()
            .RequireAuthorization();
        return app;
    }
}
