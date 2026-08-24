using ElevatorControl.Domain.Enums;
using ElevatorControl.Services;
using static ElevatorControl.Tests.TestHelpers;

namespace ElevatorControl.Tests;

public class SchedulerTests
{
    private readonly Scheduler _scheduler = new();

    // ---------- idle / empty ----------

    [Fact]
    public void Idle_with_no_stops_does_nothing()
    {
        var car = Car(1, DirectionEnum.None);

        Assert.Equal(DirectionEnum.None, _scheduler.GetNextDirection(car));
        Assert.Null(_scheduler.GetNextStop(car));
        Assert.False(_scheduler.ShouldStop(car));
    }

    [Fact]
    public void Up_with_no_stops_becomes_none()
        => Assert.Equal(DirectionEnum.None, _scheduler.GetNextDirection(Car(1, DirectionEnum.Up)));

    [Fact]
    public void Down_with_no_stops_becomes_none()
        => Assert.Equal(DirectionEnum.None, _scheduler.GetNextDirection(Car(1, DirectionEnum.Down)));

    [Fact]
    public void Idle_heads_toward_nearest_stop_above()
    {
        var car = Car(1, DirectionEnum.None, up: new[] { 3 });

        Assert.Equal(DirectionEnum.Up, _scheduler.GetNextDirection(car));
        Assert.Equal(3, _scheduler.GetNextStop(car));
    }

    [Fact]
    public void Idle_heads_toward_nearest_stop_below()
        => Assert.Equal(DirectionEnum.Down, _scheduler.GetNextDirection(Car(5, DirectionEnum.None, down: new[] { 2 })));

    [Fact]
    public void Idle_on_a_stop_serves_it()
    {
        var car = Car(3, DirectionEnum.None, up: new[] { 3 });

        Assert.True(_scheduler.ShouldStop(car));
        Assert.Equal(DirectionEnum.Up, _scheduler.GetNextDirection(car));
    }

    // ---------- LOOK going up ----------

    [Fact]
    public void Going_up_keeps_going_up_while_stops_remain_above()
    {
        var car = Car(2, DirectionEnum.Up, up: new[] { 5 });

        Assert.Equal(DirectionEnum.Up, _scheduler.GetNextDirection(car));
        Assert.Equal(5, _scheduler.GetNextStop(car));
        Assert.False(_scheduler.ShouldStop(car));
    }

    [Fact]
    public void Going_up_stops_at_an_up_stop_without_changing_direction()
    {
        var car = Car(3, DirectionEnum.Up, up: new[] { 3, 5 });

        Assert.True(_scheduler.ShouldStop(car));
        Assert.Equal(DirectionEnum.Up, _scheduler.GetNextDirection(car));
        Assert.Equal(3, _scheduler.GetNextStop(car));
    }

    [Fact]
    public void Going_up_reverses_only_after_the_up_sweep_is_done()
        => Assert.Equal(DirectionEnum.Down, _scheduler.GetNextDirection(Car(5, DirectionEnum.Up, down: new[] { 2 })));

    [Fact]
    public void Going_up_serves_a_down_call_at_the_top_as_the_turnaround()
    {
        var car = Car(5, DirectionEnum.Up, down: new[] { 2, 5 });

        Assert.True(_scheduler.ShouldStop(car));
        Assert.Equal(DirectionEnum.Down, _scheduler.GetNextDirection(car));
    }

    [Fact]
    public void Going_up_travels_to_the_highest_down_call_before_turning()
    {
        var car = Car(3, DirectionEnum.Up, down: new[] { 1, 5 });

        Assert.Equal(DirectionEnum.Up, _scheduler.GetNextDirection(car));
        Assert.Equal(5, _scheduler.GetNextStop(car));
    }

    [Fact]
    public void Going_up_deadheads_down_to_an_up_call_below_it()
    {
        var car = Car(4, DirectionEnum.Up, up: new[] { 1 });

        Assert.Equal(DirectionEnum.Down, _scheduler.GetNextDirection(car));
        Assert.Equal(1, _scheduler.GetNextStop(car));
    }

    // ---------- LOOK going down ----------

    [Fact]
    public void Going_down_keeps_going_down_while_stops_remain_below()
        => Assert.Equal(2, _scheduler.GetNextStop(Car(5, DirectionEnum.Down, down: new[] { 2 })));

    [Fact]
    public void Going_down_stops_at_a_down_stop_without_changing_direction()
    {
        var car = Car(3, DirectionEnum.Down, down: new[] { 1, 3 });

        Assert.True(_scheduler.ShouldStop(car));
        Assert.Equal(DirectionEnum.Down, _scheduler.GetNextDirection(car));
        Assert.Equal(3, _scheduler.GetNextStop(car));
    }

    [Fact]
    public void Going_down_reverses_only_after_the_down_sweep_is_done()
        => Assert.Equal(DirectionEnum.Up, _scheduler.GetNextDirection(Car(1, DirectionEnum.Down, up: new[] { 4 })));

    [Fact]
    public void Going_down_repositions_up_to_the_highest_down_call()
    {
        var car = Car(1, DirectionEnum.Down, down: new[] { 4 });

        Assert.Equal(DirectionEnum.Up, _scheduler.GetNextDirection(car));
        Assert.Equal(4, _scheduler.GetNextStop(car));
    }

    // ---------- serve direction ----------

    [Fact]
    public void A_floor_wanted_in_both_directions_keeps_the_current_heading()
    {
        var car = Car(3, DirectionEnum.Up, up: new[] { 3 }, down: new[] { 3 });

        Assert.Equal(DirectionEnum.Up, _scheduler.GetNextDirection(car));
    }
}
