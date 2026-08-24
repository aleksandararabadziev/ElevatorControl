using ElevatorControl.Domain;
using ElevatorControl.Services.Interfaces;

namespace ElevatorControl.Services;

/// <inheritdoc cref="ITrafficService"/>
public class TrafficService : ITrafficService
{
    private readonly SimulationConfig _config;
    private readonly Random _random;

    // Chance that a new passenger appears on any given tick.
    private const double SpawnChance = 0.3;

    private int _nextPassengerId = 1;

    public TrafficService(SimulationConfig config, Random? random = null)
    {
        _config = config;
        _random = random ?? new Random();
    }

    public IReadOnlyList<Passenger> GenerateTraffic()
    {
        // Most ticks are quiet; occasionally someone presses a button.
        if (_random.NextDouble() >= SpawnChance)
            return Array.Empty<Passenger>();

        var origin = _random.Next(1, _config.NumberOfFloors + 1);

        int destination;
        do
        {
            destination = _random.Next(1, _config.NumberOfFloors + 1);
        }
        while (destination == origin);

        var passenger = new Passenger
        {
            Id = _nextPassengerId++,
            OriginFloor = origin,
            DestinationFloor = destination
        };

        return new[] { passenger };
    }
}
