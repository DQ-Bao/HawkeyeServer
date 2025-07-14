using System.Data;
using Dapper;
using HawkeyeServer.Api.Models;

namespace HawkeyeServer.Api.Data;

public class PlaceDataAccess(IDbConnection db, IActivityDataAccess activities) : IPlaceDataAccess
{
    public async Task<Place> AddAsync(Place place, long tripId, IDbTransaction? tx = null) =>
        place with
        {
            Id = await db.ExecuteScalarAsync<long>(
                PlaceCommand.Add,
                new
                {
                    place.Name,
                    place.Longitude,
                    place.Latitude,
                    place.Date,
                    TripId = tripId,
                },
                transaction: tx
            ),
        };

    public async Task UpdateAsync(Place place, IDbTransaction? tx = null) =>
        await db.ExecuteAsync(
            PlaceCommand.Update,
            new
            {
                place.Id,
                place.Name,
                place.Longitude,
                place.Latitude,
                place.Date,
            },
            transaction: tx
        );

    public async Task<WithActivities<Place>> SaveAsync(
        WithActivities<Place> withActivities,
        long tripId,
        IDbTransaction? tx = null
    )
    {
        var shouldDispose = tx is null;
        tx ??= db.BeginTransaction();
        try
        {
            var place = withActivities.Place;
            if (place.Id == 0)
                place = await AddAsync(place, tripId, tx);
            else
                await UpdateAsync(place, tx);

            var updatedActivities = new List<Activity>();
            foreach (var activity in withActivities.Activities)
            {
                updatedActivities.Add(await activities.SaveAsync(activity, place.Id, tx));
            }
            if (shouldDispose)
                tx.Commit();
            return new WithActivities<Place>(place) { Activities = updatedActivities };
        }
        catch
        {
            if (shouldDispose)
                tx.Rollback();
            throw;
        }
    }
}
