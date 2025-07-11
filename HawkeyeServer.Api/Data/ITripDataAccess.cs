using HawkeyeServer.Api.Models;

namespace HawkeyeServer.Api.Data;

public static class TripCommand
{
    public const string Add =
        @"insert into trip(title, destination, start_date, end_date)
        values (@Title, @Destination, @StartDate, @EndDate)
        returning id, created_at;";
}

public interface ITripDataAccess
{
    Task<Trip> AddAsync(Trip trip);
}
