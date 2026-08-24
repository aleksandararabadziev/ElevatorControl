using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;

namespace ElevatorControl.Tests;

public class SimulationConfigTests
{
    [Fact]
    public void Defaults_match_the_building_setup()
    {
        var config = new SimulationConfig();

        Assert.Equal(5, config.NumberOfFloors);
        Assert.Equal(2, config.NumberOfCars);
        Assert.Equal(1, config.SecondsPerFloor);
        Assert.Equal(2, config.SecondsPerStop);
        Assert.Null(config.Log);
    }

    [Fact]
    public void Log_hook_can_be_invoked()
    {
        var config = new SimulationConfig();
        string? received = null;
        config.Log = message => received = message;

        config.Log?.Invoke("hello");

        Assert.Equal("hello", received);
    }

    [Fact]
    public void Direction_enum_values_support_arithmetic()
    {
        Assert.Equal(-1, (int)DirectionEnum.Down);
        Assert.Equal(0, (int)DirectionEnum.None);
        Assert.Equal(1, (int)DirectionEnum.Up);
    }
}
