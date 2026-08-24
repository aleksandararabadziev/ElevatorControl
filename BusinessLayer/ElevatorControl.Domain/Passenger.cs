using ElevatorControl.Domain.Enums;

namespace ElevatorControl.Domain;

/// <summary>
/// A single person using the elevator system.
/// </summary>
public class Passenger
{
    /// <summary>Identifier, mostly useful for logging.</summary>
    public int Id { get; set; }

    /// <summary>Floor where the passenger presses the call button and waits.</summary>
    public int OriginFloor { get; set; }

    /// <summary>Floor the passenger wants to travel to.</summary>
    public int DestinationFloor { get; set; }

    /// <summary>Direction implied by the trip (Up, Down, or None if same floor).</summary>
    public DirectionEnum Direction =>
        DestinationFloor > OriginFloor ? DirectionEnum.Up :
        DestinationFloor < OriginFloor ? DirectionEnum.Down :
        DirectionEnum.None;
}
