using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;
using ElevatorControl.Services.Interfaces;

namespace ElevatorControl.Services;

/// <inheritdoc cref="IScheduler"/>
public class Scheduler : IScheduler
{
    public int? GetNextStop(ElevatorCar car) => NextTarget(car);

    public DirectionEnum GetNextDirection(ElevatorCar car)
    {
        var target = NextTarget(car);
        if (target is null)
            return DirectionEnum.None;

        var floor = car.CurrentFloor;
        if (target > floor) return DirectionEnum.Up;
        if (target < floor) return DirectionEnum.Down;

        // Standing on the floor we need to serve: face the direction this stop belongs to.
        return ServeDirection(car, floor);
    }

    public bool ShouldStop(ElevatorCar car)
    {
        var target = NextTarget(car);
        return target.HasValue && target.Value == car.CurrentFloor;
    }

    /// <summary>
    /// The next floor the car should head to under LOOK: finish the current direction
    /// first, travel all the way to the far end before turning, and only then sweep back.
    /// Everything else (which way to face, whether to stop) is derived from this.
    /// </summary>
    private static int? NextTarget(ElevatorCar car)
    {
        var floor = car.CurrentFloor;
        return car.Direction switch
        {
            DirectionEnum.Up => NextGoingUp(car, floor),
            DirectionEnum.Down => NextGoingDown(car, floor),
            _ => NearestStop(car, floor)
        };
    }

    // While travelling up: serve the lowest up-stop at/above us; if none remain above,
    // head to the highest down-stop (the turnaround point at the top); if only up-work
    // remains below us, reposition down to the lowest of it.
    private static int? NextGoingUp(ElevatorCar car, int floor)
    {
        foreach (var f in car.UpStops) // SortedSet is ascending
            if (f >= floor)
                return f;

        if (car.DownStops.Count > 0)
            return car.DownStops.Max;

        if (car.UpStops.Count > 0)
            return car.UpStops.Min;

        return null;
    }

    // Mirror image of NextGoingUp for travelling down.
    private static int? NextGoingDown(ElevatorCar car, int floor)
    {
        int? target = null;
        foreach (var f in car.DownStops) // ascending; keep the highest one at/below us
        {
            if (f <= floor) target = f;
            else break;
        }
        if (target is not null)
            return target;

        if (car.UpStops.Count > 0)
            return car.UpStops.Min;

        if (car.DownStops.Count > 0)
            return car.DownStops.Max;

        return null;
    }

    private static int? NearestStop(ElevatorCar car, int floor)
    {
        int? nearest = null;
        var best = int.MaxValue;

        foreach (var f in car.UpStops.Concat(car.DownStops))
        {
            var distance = Math.Abs(f - floor);
            if (distance < best)
            {
                best = distance;
                nearest = f;
            }
        }

        return nearest;
    }

    private static DirectionEnum ServeDirection(ElevatorCar car, int floor)
    {
        var servesUp = car.UpStops.Contains(floor);
        var servesDown = car.DownStops.Contains(floor);

        if (servesUp && !servesDown) return DirectionEnum.Up;
        if (servesDown && !servesUp) return DirectionEnum.Down;

        return car.Direction;
    }
}
