using System.Data;
using Dapper;
using HawkeyeServer.Api.Models;

namespace HawkeyeServer.Api.Data;

public class TripDataAccess(IDbConnection db, IPlaceDataAccess places) : ITripDataAccess
{
    public async Task<Trip> AddAsync(Trip trip, IDbTransaction? tx = null)
    {
        var (id, createdAt) = await db.QuerySingleAsync<(long, DateTime)>(
            TripCommand.Add,
            new
            {
                trip.Title,
                trip.Destination,
                StartDate = trip.StartDate.ToDateTime(TimeOnly.MinValue),
                EndDate = trip.EndDate.ToDateTime(TimeOnly.MinValue),
                trip.CreatedBy,
                trip.JoinCode,
            },
            transaction: tx
        );
        return trip with { Id = id, CreatedAt = createdAt };
    }

    public async Task UpdateAsync(Trip trip, IDbTransaction? tx = null)
    {
        await db.ExecuteAsync(
            TripCommand.Update,
            new
            {
                trip.Id,
                trip.Title,
                trip.Destination,
                StartDate = trip.StartDate.ToDateTime(TimeOnly.MinValue),
                EndDate = trip.EndDate.ToDateTime(TimeOnly.MinValue),
            },
            transaction: tx
        );
    }

    public async Task<TripMate> AddMateAsync(TripMate mate) =>
        mate with
        {
            AddedAt = await db.ExecuteScalarAsync<DateTime>(
                TripCommand.AddMate,
                new { mate.TripId, mate.UserId }
            ),
        };

    public async Task<IEnumerable<User>> GetAllMatesAsync(long tripId) =>
        await db.QueryAsync<User>(TripCommand.GetAllMates, new { TripId = tripId });

    public async Task<User?> GetMateAsync(long tripId, long userId) =>
        await db.QueryFirstOrDefaultAsync<User>(
            TripCommand.GetMate,
            new { TripId = tripId, UserId = userId }
        );

    public async Task<WithPlaces<Trip>?> GetByCodeAsync(string joinCode)
    {
        var tripMap = new Dictionary<long, WithPlaces<Trip>>();
        var placeMap = new Dictionary<long, WithActivities<Place>>();
        await db.QueryAsync<Trip, Place, Activity, WithPlaces<Trip>>(
            TripCommand.GetByCode,
            (trip, place, activity) =>
            {
                if (!tripMap.TryGetValue(trip.Id, out var withPlaces))
                {
                    withPlaces = new WithPlaces<Trip>(trip);
                    tripMap[trip.Id] = withPlaces;
                }
                if (place is { Id: > 0 } && !placeMap.TryGetValue(place.Id, out var withActivities))
                {
                    withActivities = new WithActivities<Place>(place);
                    placeMap[place.Id] = withActivities;
                    withPlaces.Places.Add(withActivities);
                }
                if (activity is { Id: > 0 } && place?.Id > 0)
                {
                    placeMap[place.Id].Activities.Add(activity);
                }
                return withPlaces;
            },
            new { JoinCode = joinCode },
            splitOn: "PlaceId,ActivityId"
        );
        return tripMap.Values.FirstOrDefault();
    }

    public async Task<WithPlaces<Trip>?> GetByIdAsync(long id)
    {
        var tripMap = new Dictionary<long, WithPlaces<Trip>>();
        var placeMap = new Dictionary<long, WithActivities<Place>>();
        await db.QueryAsync<Trip, Place, Activity, WithPlaces<Trip>>(
            TripCommand.GetById,
            (trip, place, activity) =>
            {
                if (!tripMap.TryGetValue(trip.Id, out var withPlaces))
                {
                    withPlaces = new WithPlaces<Trip>(trip);
                    tripMap[trip.Id] = withPlaces;
                }
                if (place is { Id: > 0 } && !placeMap.TryGetValue(place.Id, out var withActivities))
                {
                    withActivities = new WithActivities<Place>(place);
                    placeMap[place.Id] = withActivities;
                    withPlaces.Places.Add(withActivities);
                }
                if (activity is { Id: > 0 } && place?.Id > 0)
                {
                    placeMap[place.Id].Activities.Add(activity);
                }
                return withPlaces;
            },
            new { Id = id },
            splitOn: "PlaceId,ActivityId"
        );
        return tripMap.Values.FirstOrDefault();
    }

    public async Task<WithPlaces<Trip>> SaveAsync(WithPlaces<Trip> withPlaces)
    {
        using var transaction = db.BeginTransaction();

        var trip = withPlaces.Trip;
        if (trip.Id == 0)
            trip = await AddAsync(trip, transaction);
        else
            await UpdateAsync(trip, transaction);

        var updatedPlaces = new List<WithActivities<Place>>();
        foreach (var withActivities in withPlaces.Places)
            updatedPlaces.Add(await places.SaveAsync(withActivities, trip.Id, transaction));

        transaction.Commit();

        return new WithPlaces<Trip>(trip) { Places = updatedPlaces };
    }
}
