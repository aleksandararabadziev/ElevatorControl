using ElevatorControl.Domain.Enums;

namespace ElevatorControl.Domain;

/// <summary>
/// A single elevator car and the state it needs to move people around.
/// </summary>
public class ElevatorCar
{
    /// <summary>Identifier, mostly useful for logging.</summary>
    public int Id { get; set; }

    /// <summary>Floor the car is currently at.</summary>
    public int CurrentFloor { get; set; }

    /// <summary>Direction the car is currently travelling.</summary>
    public DirectionEnum Direction { get; set; } = DirectionEnum.None;

    /// <summary>What the car is currently doing.</summary>
    public CarStateEnum State { get; set; } = CarStateEnum.Idle;

    /// <summary>Floors the car still needs to stop at while going up (naturally ordered ascending).</summary>
    public SortedSet<int> UpStops { get; } = new();

    /// <summary>Floors the car still needs to stop at while going down (served in descending order).</summary>
    public SortedSet<int> DownStops { get; } = new();

    /// <summary>Passengers currently riding inside the car.</summary>
    public List<Passenger> Passengers { get; } = new();
}
