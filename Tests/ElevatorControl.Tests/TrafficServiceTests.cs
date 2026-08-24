using ElevatorControl.Domain;
using ElevatorControl.Services;

namespace ElevatorControl.Tests;

public class TrafficServiceTests
{
    private static List<Passenger> Generate(TrafficService service, int ticks, out int emptyTicks)
    {
        var all = new List<Passenger>();
        emptyTicks = 0;
        for (var i = 0; i < ticks; i++)
        {
            var batch = service.GenerateTraffic();
            if (batch.Count == 0) emptyTicks++;
            all.AddRange(batch);
        }
        return all;
    }

    [Fact]
    public void Generates_valid_passengers_over_time()
    {
        var config = new SimulationConfig { NumberOfFloors = 5 };
        var service = new TrafficService(config, new Random(42));

        var passengers = Generate(service, 200, out var emptyTicks);

        Assert.NotEmpty(passengers);                 // some ticks produce demand
        Assert.True(emptyTicks > 0);                 // and some are quiet
        Assert.All(passengers, p =>
        {
            Assert.InRange(p.OriginFloor, 1, config.NumberOfFloors);
            Assert.InRange(p.DestinationFloor, 1, config.NumberOfFloors);
            Assert.NotEqual(p.OriginFloor, p.DestinationFloor);
        });
    }

    [Fact]
    public void Passenger_ids_increase_sequentially()
    {
        var service = new TrafficService(new SimulationConfig { NumberOfFloors = 5 }, new Random(42));

        var passengers = Generate(service, 200, out _);

        Assert.Equal(Enumerable.Range(1, passengers.Count), passengers.Select(p => p.Id));
    }

    [Fact]
    public void Works_without_an_explicit_random()
    {
        var service = new TrafficService(new SimulationConfig { NumberOfFloors = 5 });

        var passengers = Generate(service, 200, out _);

        Assert.All(passengers, p =>
        {
            Assert.InRange(p.OriginFloor, 1, 5);
            Assert.InRange(p.DestinationFloor, 1, 5);
            Assert.NotEqual(p.OriginFloor, p.DestinationFloor);
        });
    }

    [Fact]
    public void Same_seed_produces_the_same_traffic()
    {
        var config = new SimulationConfig { NumberOfFloors = 5 };
        var a = Generate(new TrafficService(config, new Random(7)), 100, out _);
        var b = Generate(new TrafficService(config, new Random(7)), 100, out _);

        Assert.Equal(a.Count, b.Count);
        Assert.Equal(
            a.Select(p => (p.OriginFloor, p.DestinationFloor)),
            b.Select(p => (p.OriginFloor, p.DestinationFloor)));
    }
}
