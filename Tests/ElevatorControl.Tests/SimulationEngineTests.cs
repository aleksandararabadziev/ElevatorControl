using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;
using ElevatorControl.Services;

namespace ElevatorControl.Tests;

public class SimulationEngineTests
{
    private static SimulationEngine BuildEngine(SimulationConfig config, params Passenger[] scripted)
        => new(config, new Scheduler(), new Dispatcher(config), new ScriptedTrafficService(scripted));

    private static bool IsSettled(SimulationEngine engine)
        => engine.WaitingPassengers.Count == 0
           && engine.Cars.All(c => c.Passengers.Count == 0 && c.State == CarStateEnum.Idle);

    [Fact]
    public void Builds_the_configured_cars_all_idle_at_floor_one()
    {
        var config = new SimulationConfig { NumberOfCars = 2 };
        var engine = BuildEngine(config);

        Assert.Equal(2, engine.Cars.Count);
        Assert.All(engine.Cars, c =>
        {
            Assert.Equal(1, c.CurrentFloor);
            Assert.Equal(CarStateEnum.Idle, c.State);
            Assert.Equal(DirectionEnum.None, c.Direction);
        });
    }

    [Fact]
    public void Step_advances_the_clock()
    {
        var engine = BuildEngine(new SimulationConfig());

        Assert.Equal(0, engine.ElapsedSeconds);
        engine.Step();
        Assert.Equal(1, engine.ElapsedSeconds);
    }

    [Fact]
    public void A_car_only_picks_up_passengers_going_its_current_direction()
    {
        var config = new SimulationConfig { NumberOfFloors = 5, NumberOfCars = 1, SecondsPerFloor = 1, SecondsPerStop = 1 };
        var engine = BuildEngine(config,
            new Passenger { Id = 1, OriginFloor = 3, DestinationFloor = 5 },  // going Up
            new Passenger { Id = 2, OriginFloor = 3, DestinationFloor = 1 }); // going Down

        var checkedBoarding = false;
        for (var i = 0; i < 100 && !(checkedBoarding && IsSettled(engine) && engine.ElapsedSeconds > 1); i++)
        {
            engine.Step();

            var car = engine.Cars[0];
            if (!checkedBoarding && car.CurrentFloor == 3 && car.State == CarStateEnum.DoorsOpen)
            {
                checkedBoarding = true;
                // The car is heading up here, so only the Up passenger boards.
                Assert.Contains(car.Passengers, p => p.Id == 1);
                Assert.DoesNotContain(car.Passengers, p => p.Id == 2);
                Assert.Contains(engine.WaitingPassengers, p => p.Id == 2);
            }
        }

        Assert.True(checkedBoarding, "the car should have opened its doors at floor 3");
        Assert.True(IsSettled(engine), "everyone should be delivered by the end");
    }

    [Fact]
    public void Reports_events_through_the_log_hook()
    {
        var log = new List<string>();
        var config = new SimulationConfig { NumberOfCars = 1, Log = log.Add };
        var engine = BuildEngine(config, new Passenger { Id = 1, OriginFloor = 1, DestinationFloor = 4 });

        for (var i = 0; i < 50 && !(IsSettled(engine) && engine.ElapsedSeconds > 1); i++)
            engine.Step();

        Assert.NotEmpty(log);
    }

    [Fact]
    public void Delivers_everyone_and_winds_down()
    {
        var config = new SimulationConfig { NumberOfFloors = 5, NumberOfCars = 2, SecondsPerFloor = 1, SecondsPerStop = 2 };
        var engine = BuildEngine(config,
            new Passenger { Id = 1, OriginFloor = 1, DestinationFloor = 5 },
            new Passenger { Id = 2, OriginFloor = 4, DestinationFloor = 2 },
            new Passenger { Id = 3, OriginFloor = 2, DestinationFloor = 3 },
            new Passenger { Id = 4, OriginFloor = 5, DestinationFloor = 1 });

        var settled = false;
        for (var i = 0; i < 300; i++)
        {
            engine.Step();
            if (IsSettled(engine) && engine.ElapsedSeconds > 1)
            {
                settled = true;
                break;
            }
        }

        Assert.True(settled, "the simulation should drain all demand and go idle");
    }
}
