using System.Collections.Concurrent;
using HawkeyeServer.Api.Models;

namespace HawkeyeServer.Api.Data;

public class TripMemoryStore
{
    private readonly ConcurrentDictionary<long, WithPlaces<Trip>> _trips = [];

    public WithPlaces<Trip>? GetTrip(long tripId) =>
        _trips.TryGetValue(tripId, out var trip) ? trip : null;

    public void SetTrip(long tripId, WithPlaces<Trip> trip) => _trips[tripId] = trip;

    public void UpdateTrip(long tripId, Action<WithPlaces<Trip>> update)
    {
        if (_trips.TryGetValue(tripId, out var trip))
        {
            update(trip);
        }
    }

    public IEnumerable<KeyValuePair<long, WithPlaces<Trip>>> GetAllTrips() => _trips;

    public (bool success, WithPlaces<Trip>? trip) RemoveTrip(long tripId) =>
        (_trips.TryRemove(tripId, out var trip), trip);
}
