namespace ElevatorControl.Domain;

/// <summary>
/// Settings that describe the building and how the simulation runs.
/// </summary>
public class SimulationConfig
{
    /// <summary>Number of floors in the building.</summary>
    public int NumberOfFloors { get; set; } = 5;

    /// <summary>Number of elevator cars serving the building.</summary>
    public int NumberOfCars { get; set; } = 2;

    /// <summary>Simulated seconds it takes a car to travel one floor.</summary>
    public int SecondsPerFloor { get; set; } = 1;

    /// <summary>Simulated seconds a car spends stopped at a floor (doors open then close).</summary>
    public int SecondsPerStop { get; set; } = 2;

    /// <summary>
    /// Logging hook the simulation calls to report events (e.g. "Car 1 arrived at floor 3").
    /// Left null means no logging.
    /// </summary>
    public Action<string>? Log { get; set; }
}
