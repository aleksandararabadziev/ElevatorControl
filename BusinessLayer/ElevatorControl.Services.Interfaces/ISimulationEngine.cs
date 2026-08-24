using ElevatorControl.Domain;

namespace ElevatorControl.Services.Interfaces;

/// <summary>
/// Drives the whole simulation one tick (one simulated second) at a time:
/// it creates traffic, dispatches new calls, and moves the cars while keeping
/// their live state up to date.
/// </summary>
public interface ISimulationEngine
{
    /// <summary>The elevator cars and their current live state.</summary>
    IReadOnlyList<ElevatorCar> Cars { get; }

    /// <summary>Passengers who have called a car but have not been picked up yet.</summary>
    IReadOnlyList<Passenger> WaitingPassengers { get; }

    /// <summary>Simulated seconds elapsed since the simulation started.</summary>
    int ElapsedSeconds { get; }

    /// <summary>Advances the simulation by one tick.</summary>
    void Step();
}
