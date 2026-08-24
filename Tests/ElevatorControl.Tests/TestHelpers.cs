using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;
using ElevatorControl.Services.Interfaces;

namespace ElevatorControl.Tests;

internal static class TestHelpers
{
    /// <summary>Builds a car with the given position, heading and pending stops.</summary>
    public static ElevatorCar Car(int floor, DirectionEnum direction, int[]? up = null, int[]? down = null)
    {
        var car = new ElevatorCar { Id = 1, CurrentFloor = floor, Direction = direction };
        foreach (var f in up ?? Array.Empty<int>()) car.UpStops.Add(f);
        foreach (var f in down ?? Array.Empty<int>()) car.DownStops.Add(f);
        return car;
    }
}

/// <summary>Traffic double: emits a fixed set of passengers on the first tick, then nothing.</summary>
internal sealed class ScriptedTrafficService : ITrafficService
{
    private readonly Passenger[] _first;
    private bool _done;

    public ScriptedTrafficService(params Passenger[] first) => _first = first;

    public IReadOnlyList<Passenger> GenerateTraffic()
    {
        if (_done) return Array.Empty<Passenger>();
        _done = true;
        return _first;
    }
}
