using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;
using ElevatorControl.Services;
using static ElevatorControl.Tests.TestHelpers;

namespace ElevatorControl.Tests;

public class DispatcherTests
{
    private readonly Dispatcher _dispatcher =
        new(new SimulationConfig { NumberOfFloors = 5, NumberOfCars = 2 });

    [Fact]
    public void No_cars_returns_null()
        => Assert.Null(_dispatcher.Assign(new List<ElevatorCar>(), 3, DirectionEnum.Up));

    [Fact]
    public void Idle_car_gets_an_up_call_recorded_as_an_up_stop()
    {
        var car = Car(1, DirectionEnum.None);

        var chosen = _dispatcher.Assign(new[] { car }, 3, DirectionEnum.Up);

        Assert.Same(car, chosen);
        Assert.Contains(3, car.UpStops);
    }

    [Fact]
    public void Idle_car_gets_a_down_call_recorded_as_a_down_stop()
    {
        var car = Car(5, DirectionEnum.None);

        var chosen = _dispatcher.Assign(new[] { car }, 2, DirectionEnum.Down);

        Assert.Same(car, chosen);
        Assert.Contains(2, car.DownStops);
    }

    [Fact]
    public void Idle_car_is_preferred_over_a_car_heading_the_wrong_way()
    {
        var wrongWay = Car(1, DirectionEnum.Up);
        var idle = Car(5, DirectionEnum.None);

        var chosen = _dispatcher.Assign(new[] { wrongWay, idle }, 4, DirectionEnum.Down);

        Assert.Same(idle, chosen);
    }

    [Fact]
    public void Approaching_car_beats_a_closer_car_going_the_wrong_way()
    {
        var approaching = Car(2, DirectionEnum.Up);      // same direction, call ahead
        var closerWrongWay = Car(5, DirectionEnum.Down); // closer, but wrong way

        var chosen = _dispatcher.Assign(new[] { approaching, closerWrongWay }, 4, DirectionEnum.Up);

        Assert.Same(approaching, chosen);
    }

    [Fact]
    public void Same_direction_but_passed_still_beats_the_wrong_way()
    {
        var passed = Car(4, DirectionEnum.Up);   // going up but already above the call
        var wrongWay = Car(4, DirectionEnum.Down);

        var chosen = _dispatcher.Assign(new[] { passed, wrongWay }, 2, DirectionEnum.Up);

        Assert.Same(passed, chosen);
    }

    [Fact]
    public void Ties_pick_the_first_car()
    {
        var first = Car(1, DirectionEnum.None);
        var second = Car(1, DirectionEnum.None);

        var chosen = _dispatcher.Assign(new[] { first, second }, 1, DirectionEnum.Up);

        Assert.Same(first, chosen);
    }
}
