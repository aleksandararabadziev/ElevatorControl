using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;
using ElevatorControl.Services.Interfaces;

namespace ElevatorControl.Services;

/// <inheritdoc cref="ISimulationEngine"/>
public class SimulationEngine : ISimulationEngine
{
    private readonly SimulationConfig _config;
    private readonly IScheduler _scheduler;
    private readonly IDispatcher _dispatcher;
    private readonly ITrafficService _traffic;

    private readonly List<ElevatorCar> _cars = new();
    private readonly List<Passenger> _waiting = new();

    // Seconds left before each car finishes its current move / door cycle, keyed by car id.
    private readonly Dictionary<int, int> _busySeconds = new();

    public SimulationEngine(
        SimulationConfig config,
        IScheduler scheduler,
        IDispatcher dispatcher,
        ITrafficService traffic)
    {
        _config = config;
        _scheduler = scheduler;
        _dispatcher = dispatcher;
        _traffic = traffic;

        for (var id = 1; id <= config.NumberOfCars; id++)
        {
            _cars.Add(new ElevatorCar { Id = id, CurrentFloor = 1 });
            _busySeconds[id] = 0;
        }
    }

    public IReadOnlyList<ElevatorCar> Cars => _cars;
    public IReadOnlyList<Passenger> WaitingPassengers => _waiting;
    public int ElapsedSeconds { get; private set; }

    public void Step()
    {
        SpawnAndDispatch();

        foreach (var car in _cars)
            Advance(car);

        ElapsedSeconds++;
    }

    private void SpawnAndDispatch()
    {
        foreach (var passenger in _traffic.GenerateTraffic())
        {
            _waiting.Add(passenger);
            Log($"Passenger {passenger.Id} calls at floor {passenger.OriginFloor} going {passenger.Direction} (wants floor {passenger.DestinationFloor}).");

            var car = _dispatcher.Assign(_cars, passenger.OriginFloor, passenger.Direction);
            if (car is not null)
                Log($"  -> assigned to car {car.Id}.");
        }
    }

    private void Advance(ElevatorCar car)
    {
        // Still finishing a timed action (moving a floor, or holding the doors open)?
        if (_busySeconds[car.Id] > 0)
        {
            _busySeconds[car.Id]--;
            if (_busySeconds[car.Id] > 0)
                return;

            // The action completes on this tick.
            if (car.State == CarStateEnum.Moving)
                car.CurrentFloor += (int)car.Direction; // arrived at the next floor
        }

        // The car is now sitting at a floor and free to decide what to do next.
        // LOOK: we only re-evaluate the direction here, never mid-move.
        car.Direction = _scheduler.GetNextDirection(car);

        if (_scheduler.ShouldStop(car))
        {
            ServeFloor(car);
            car.State = CarStateEnum.DoorsOpen;
            _busySeconds[car.Id] = _config.SecondsPerStop;
            return;
        }

        if (car.Direction == DirectionEnum.None)
        {
            if (car.State != CarStateEnum.Idle)
                Log($"Car {car.Id} is idle at floor {car.CurrentFloor}.");
            car.State = CarStateEnum.Idle;
            return;
        }

        car.State = CarStateEnum.Moving;
        _busySeconds[car.Id] = _config.SecondsPerFloor;
    }

    private void ServeFloor(ElevatorCar car)
    {
        var floor = car.CurrentFloor;
        Log($"Car {car.Id} stops at floor {floor} ({car.Direction}), doors open.");

        // Passengers getting off here.
        var arriving = car.Passengers.Where(p => p.DestinationFloor == floor).ToList();
        foreach (var passenger in arriving)
        {
            car.Passengers.Remove(passenger);
            Log($"  Passenger {passenger.Id} leaves car {car.Id} at floor {floor}.");
        }

        // Passengers getting on - only those travelling the car's current direction.
        var boarding = _waiting
            .Where(p => p.OriginFloor == floor && p.Direction == car.Direction)
            .ToList();
        foreach (var passenger in boarding)
        {
            _waiting.Remove(passenger);
            car.Passengers.Add(passenger);
            AddStop(car, passenger.DestinationFloor, passenger.Direction);
            Log($"  Passenger {passenger.Id} boards car {car.Id}, heading to floor {passenger.DestinationFloor}.");
        }

        // This floor has now been served for the current direction.
        RemoveStop(car, floor);
    }

    private static void AddStop(ElevatorCar car, int floor, DirectionEnum direction)
    {
        if (direction == DirectionEnum.Up) car.UpStops.Add(floor);
        else if (direction == DirectionEnum.Down) car.DownStops.Add(floor);
    }

    private static void RemoveStop(ElevatorCar car, int floor)
    {
        switch (car.Direction)
        {
            case DirectionEnum.Up:
                car.UpStops.Remove(floor);
                break;
            case DirectionEnum.Down:
                car.DownStops.Remove(floor);
                break;
            default:
                car.UpStops.Remove(floor);
                car.DownStops.Remove(floor);
                break;
        }
    }

    private void Log(string message) => _config.Log?.Invoke(message);
}
