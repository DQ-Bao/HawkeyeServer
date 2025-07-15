using HawkeyeServer.Api.Data;

namespace HawkeyeServer.Api.Services;

public class TripPersistService(
    TripMemoryStore memory,
    IServiceScopeFactory scopeFactory,
    ILogger<TripPersistService> logger
) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_interval, stoppingToken);
            using var scope = scopeFactory.CreateScope();
            var trips = scope.ServiceProvider.GetRequiredService<ITripDataAccess>();
            foreach (var (tripId, trip) in memory.GetAllTrips())
            {
                try
                {
                    memory.SetTrip(tripId, await trips.SaveAsync(trip));
                }
                catch (Exception e)
                {
                    logger.LogError(e, $"Failed to persist trip {tripId}");
                }
            }
        }
    }
}
