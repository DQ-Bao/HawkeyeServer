using System.Data;
using Dapper;
using HawkeyeServer.Api.Models;

namespace HawkeyeServer.Api.Data;

public class TripDataAccess(IDbConnection db) : ITripDataAccess
{
    public async Task<Trip> AddAsync(Trip trip)
    {
        var (id, createdAt) = await db.QuerySingleAsync<(long, DateTime)>(
            TripCommand.Add,
            new
            {
                trip.Title,
                trip.Destination,
                StartDate = trip.StartDate.ToDateTime(TimeOnly.MinValue),
                EndDate = trip.EndDate.ToDateTime(TimeOnly.MinValue),
            }
        );
        return trip with { Id = id, CreatedAt = createdAt };
    }
}
