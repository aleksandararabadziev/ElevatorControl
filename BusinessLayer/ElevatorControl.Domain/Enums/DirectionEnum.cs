namespace ElevatorControl.Domain.Enums;

/// <summary>
/// Direction a car (or a passenger's trip) is heading.
/// Values are chosen so they can be used in arithmetic (e.g. next floor = current + (int)direction).
/// </summary>
public enum DirectionEnum
{
    Down = -1,
    None = 0,
    Up = 1
}
