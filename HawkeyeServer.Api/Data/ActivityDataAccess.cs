using System.Data;
using Dapper;
using HawkeyeServer.Api.Models;

namespace HawkeyeServer.Api.Data;

public class ActivityDataAccess(IDbConnection db) : IActivityDataAccess
{
    public async Task<Activity> AddAsync(
        Activity activity,
        long placeId,
        IDbTransaction? tx = null
    ) =>
        activity with
        {
            Id = await db.ExecuteScalarAsync<long>(
                ActivityCommand.Add,
                new
                {
                    activity.Name,
                    activity.StartTime,
                    PlaceId = placeId,
                },
                transaction: tx
            ),
        };

    public async Task UpdateAsync(Activity activity, IDbTransaction? tx = null) =>
        await db.ExecuteAsync(
            ActivityCommand.Update,
            new
            {
                activity.Id,
                activity.Name,
                activity.StartTime,
            },
            transaction: tx
        );

    public async Task<Activity> SaveAsync(
        Activity activity,
        long placeId,
        IDbTransaction? tx = null
    )
    {
        var updated = activity;
        if (activity.Id == 0)
            updated = await AddAsync(activity, placeId, tx);
        else
            await UpdateAsync(activity, tx);
        return updated;
    }
}
