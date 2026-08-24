using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;
using ElevatorControl.Services.Interfaces;

namespace ElevatorControl.Services;

/// <inheritdoc cref="IDispatcher"/>
public class Dispatcher : IDispatcher
{
    private readonly SimulationConfig _config;

    public Dispatcher(SimulationConfig config)
    {
        _config = config;
    }

    public ElevatorCar? Assign(IReadOnlyList<ElevatorCar> cars, int callFloor, DirectionEnum callDirection)
    {
        if (cars is null || cars.Count == 0)
            return null;

        ElevatorCar? best = null;
        var bestCost = int.MaxValue;

        foreach (var car in cars)
        {
            var cost = EstimateCost(car, callFloor, callDirection);
            if (cost < bestCost)
            {
                bestCost = cost;
                best = car;
            }
        }

        if (best is not null)
            AddStop(best, callFloor, callDirection);

        return best;
    }

    private int EstimateCost(ElevatorCar car, int callFloor, DirectionEnum callDirection)
    {
        var distance = Math.Abs(car.CurrentFloor - callFloor);
        var floors = _config.NumberOfFloors;

        // Idle
        if (car.Direction == DirectionEnum.None)
            return distance;

        var sameDirection = car.Direction == callDirection;
        var callIsAhead =
            (car.Direction == DirectionEnum.Up && callFloor >= car.CurrentFloor) ||
            (car.Direction == DirectionEnum.Down && callFloor <= car.CurrentFloor);

        // Approaching the floor
        if (sameDirection && callIsAhead)
            return distance;

        // Same direction but already passed the floor
        if (sameDirection)
            return distance + floors;

        // Heading the other direction
        return distance + (2 * floors);
    }

    private static void AddStop(ElevatorCar car, int floor, DirectionEnum direction)
    {
        if (direction == DirectionEnum.Up)
            car.UpStops.Add(floor);
        else if (direction == DirectionEnum.Down)
            car.DownStops.Add(floor);
    }
}
