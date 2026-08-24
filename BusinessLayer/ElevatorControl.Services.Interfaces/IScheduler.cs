using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;

namespace ElevatorControl.Services.Interfaces;

/// <summary>
/// Decides how a single car moves, following the LOOK rule:
/// keep travelling in the current direction while there are still stops ahead,
/// and only reverse once that direction has been fully served.
/// </summary>
public interface IScheduler
{
    /// <summary>Direction the car should travel next (None when it has no work to do).</summary>
    DirectionEnum GetNextDirection(ElevatorCar car);

    /// <summary>True if the car should stop at its current floor.</summary>
    bool ShouldStop(ElevatorCar car);

    /// <summary>The next floor the car should head to, or null when there is nothing to serve.</summary>
    int? GetNextStop(ElevatorCar car);
}
