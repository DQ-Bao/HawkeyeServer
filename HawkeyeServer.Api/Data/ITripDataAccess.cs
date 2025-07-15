using System.Data;
using HawkeyeServer.Api.Models;

namespace HawkeyeServer.Api.Data;

public static class TripCommand
{
    public const string Add =
        @"insert into trip(title, destination, start_date, end_date, created_by, join_code)
        values (@Title, @Destination, @StartDate, @EndDate, @CreatedBy, @JoinCode)
        returning id, created_at;";
    public const string Update =
        @"update trip
          set title = @Title,
              destination = @Destination,
              start_date = @StartDate,
              end_date = @EndDate
          where id = @Id;";
    public const string AddMate =
        @"insert into trip_mate(trip_id, user_id)
        values (@TripId, @UserId)
        returning added_at;";
    public const string GetAllMates =
        @"select u.* from users u
        join trip_mate tm on tm.user_id = u.id
        where tm.trip_id = @TripId";
    public const string GetMate =
        @"select u.* from users u
        join trip_mate tm on u.id = tm.user_id
        where tm.trip_id = @TripId and tm.user_id = @UserId;";

    public const string GetByCode =
        @"select 
        t.id as TripId, t.created_by, t.title, t.destination, t.start_date, t.end_date, t.join_code, t.created_at,
        p.id as PlaceId, p.name as PlaceName, p.longitude, p.latitude, p.date, p.trip_id,
        a.id as ActivityId, a.name as ActivityName, a.start_time, a.place_id
        from trip t
        left join place p on p.trip_id = t.id
        left join activity a on a.place_id = p.id
        where t.join_code = @JoinCode;";
    public const string GetById =
        @"select
        t.id as TripId, t.created_by, t.title, t.destination, t.start_date, t.end_date, t.join_code, t.created_at,
        p.id as PlaceId, p.name as PlaceName, p.longitude, p.latitude, p.date, p.trip_id,
        a.id as ActivityId, a.name as ActivityName, a.start_time, a.place_id
        from trip t
        left join place p on p.trip_id = t.id
        left join activity a on a.place_id = p.id
        where t.id = @Id;";
    public const string GetAllByUser = @"select * from trip where created_by = @UserId";
}

public interface ITripDataAccess
{
    Task<Trip> AddAsync(Trip trip, IDbTransaction? tx = null);
    Task UpdateAsync(Trip trip, IDbTransaction? tx = null);
    Task<TripMate> AddMateAsync(TripMate mate);
    Task<IEnumerable<User>> GetAllMatesAsync(long tripId);
    Task<User?> GetMateAsync(long tripId, long userId);
    Task<WithPlaces<Trip>?> GetByCodeAsync(string joinCode);
    Task<WithPlaces<Trip>?> GetByIdAsync(long id);
    Task<WithPlaces<Trip>> SaveAsync(WithPlaces<Trip> withPlaces);
    Task<IEnumerable<Trip>> GetAllByUser(long userId);
}
