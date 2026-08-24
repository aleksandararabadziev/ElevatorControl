using ElevatorControl.Domain;
using ElevatorControl.Services.Interfaces;

namespace ElevatorControl.ConsoleApp;

/// <summary>
/// Wraps a traffic service and lets it generate calls only for a limited number of
/// ticks. After the window closes it produces no more passengers, so the simulation
/// can drain the remaining demand and wind down.
/// </summary>
public class TimeLimitedTrafficService : ITrafficService
{
    private readonly ITrafficService _inner;
    private readonly int _maxTicks;
    private int _ticks;

    public TimeLimitedTrafficService(ITrafficService inner, int maxTicks)
    {
        _inner = inner;
        _maxTicks = maxTicks;
    }

    public IReadOnlyList<Passenger> GenerateTraffic()
    {
        if (_ticks >= _maxTicks)
            return Array.Empty<Passenger>();

        _ticks++;
        return _inner.GenerateTraffic();
    }
}
