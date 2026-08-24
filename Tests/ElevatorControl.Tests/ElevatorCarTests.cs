using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;

namespace ElevatorControl.Tests;

public class ElevatorCarTests
{
    [Fact]
    public void New_car_starts_idle_with_no_work()
    {
        var car = new ElevatorCar();

        Assert.Equal(CarStateEnum.Idle, car.State);
        Assert.Equal(DirectionEnum.None, car.Direction);
        Assert.Empty(car.UpStops);
        Assert.Empty(car.DownStops);
        Assert.Empty(car.Passengers);
    }
}
