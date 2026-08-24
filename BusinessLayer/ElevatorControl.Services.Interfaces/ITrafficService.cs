using ElevatorControl.Domain;

namespace ElevatorControl.Services.Interfaces;

/// <summary>
/// Produces random passenger demand over time. Each passenger is a hall call at an
/// origin floor plus a destination stop, so a single passenger represents both a call
/// and a stop.
/// </summary>
public interface ITrafficService
{
    /// <summary>Returns any new passengers that appear during the current tick (may be empty).</summary>
    IReadOnlyList<Passenger> GenerateTraffic();
}
