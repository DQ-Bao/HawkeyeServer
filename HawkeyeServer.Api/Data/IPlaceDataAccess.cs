using System.Data;
using HawkeyeServer.Api.Models;

namespace HawkeyeServer.Api.Data;

public static class PlaceCommand
{
    public const string Add =
        @"insert into place (name, longitude, latitude, date, trip_id)
          values (@Name, @Longitude, @Latitude, @Date, @TripId)
          returning id;";
    public const string Update =
        @"update place
          set name = @Name,
              longitude = @Longitude,
              latitude = @Latitude,
              date = @Date
          where id = @Id;";
}

public interface IPlaceDataAccess
{
    Task<Place> AddAsync(Place place, long tripId, IDbTransaction? tx = null);
    Task UpdateAsync(Place place, IDbTransaction? tx = null);
    Task<WithActivities<Place>> SaveAsync(
        WithActivities<Place> withActivities,
        long tripId,
        IDbTransaction? tx = null
    );
}
