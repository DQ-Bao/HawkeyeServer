using System.Data;
using HawkeyeServer.Api.Models;

namespace HawkeyeServer.Api.Data;

public static class ActivityCommand
{
    public const string Add =
        @"insert into activity (name, start_time, place_id)
          values (@Name, @StartTime, @PlaceId)
          returning id;";
    public const string Update =
        @"update activity
          set name = @Name,
              start_time = @StartTime,
          where id = @Id;";
}

public interface IActivityDataAccess
{
    Task<Activity> AddAsync(Activity activity, long placeId, IDbTransaction? tx = null);
    Task UpdateAsync(Activity activity, IDbTransaction? tx = null);
    Task<Activity> SaveAsync(Activity activity, long placeId, IDbTransaction? tx = null);
}
