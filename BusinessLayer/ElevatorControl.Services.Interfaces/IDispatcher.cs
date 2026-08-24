using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;

namespace ElevatorControl.Services.Interfaces;

/// <summary>
/// Assigns hall calls to the most suitable elevator car.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Picks the best car for a call made at <paramref name="callFloor"/> heading
    /// <paramref name="callDirection"/>, records the stop on that car, and returns it.
    /// Returns null when there are no cars to choose from.
    /// </summary>
    ElevatorCar? Assign(IReadOnlyList<ElevatorCar> cars, int callFloor, DirectionEnum callDirection);
}
