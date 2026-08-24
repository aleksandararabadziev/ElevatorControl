using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;

namespace ElevatorControl.Tests;

public class PassengerTests
{
    [Theory]
    [InlineData(1, 5, DirectionEnum.Up)]
    [InlineData(5, 1, DirectionEnum.Down)]
    [InlineData(3, 3, DirectionEnum.None)]
    public void Direction_is_derived_from_origin_and_destination(int origin, int destination, DirectionEnum expected)
    {
        var passenger = new Passenger { OriginFloor = origin, DestinationFloor = destination };

        Assert.Equal(expected, passenger.Direction);
    }
}
