using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;
using ElevatorControl.Services.Interfaces;

namespace ElevatorControl.ConsoleApp;

/// <summary>
/// Draws a simple live dashboard: where the cars are, what they are doing, who is
/// waiting on each floor, and the most recent events.
/// </summary>
public static class ConsoleRenderer
{
    public static void Render(
        ISimulationEngine engine,
        SimulationConfig config,
        int trafficWindowSeconds,
        IReadOnlyList<string> recentEvents)
    {
        // Only clear when we have a real console; when output is redirected we just scroll.
        if (!Console.IsOutputRedirected)
            Console.Clear();

        var trafficOn = engine.ElapsedSeconds < trafficWindowSeconds;
        var trafficText = trafficOn
            ? $"ON ({trafficWindowSeconds - engine.ElapsedSeconds}s left)"
            : "OFF";

        Console.WriteLine($"ElevatorControl   t={engine.ElapsedSeconds}s   traffic:{trafficText}   waiting:{engine.WaitingPassengers.Count}");
        Console.WriteLine(new string('-', 62));

        var waitingByFloor = engine.WaitingPassengers
            .GroupBy(p => p.OriginFloor)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Building, drawn top floor first.
        for (var floor = config.NumberOfFloors; floor >= 1; floor--)
        {
            var carsHere = string.Join(" ", engine.Cars
                .Where(c => c.CurrentFloor == floor)
                .Select(c => $"[C{c.Id}{StateGlyph(c)}]"));

            var waitingCell = waitingByFloor.TryGetValue(floor, out var people)
                ? "wait: " + string.Join(" ", people.Select(p => $"P{p.Id}{Arrow(p.Direction)}"))
                : string.Empty;

            Console.WriteLine($"F{floor,2} | {carsHere,-16}| {waitingCell}");
        }

        Console.WriteLine();
        Console.WriteLine("Cars:");
        foreach (var car in engine.Cars)
        {
            var dests = car.Passengers.Count == 0
                ? "-"
                : string.Join(",", car.Passengers.Select(p => p.DestinationFloor).OrderBy(f => f));
            var up = car.UpStops.Count == 0 ? "-" : string.Join(",", car.UpStops);
            var down = car.DownStops.Count == 0 ? "-" : string.Join(",", car.DownStops);

            Console.WriteLine(
                $"  C{car.Id}  floor {car.CurrentFloor}  {car.State,-9} {Arrow(car.Direction)}  " +
                $"pax:{car.Passengers.Count} dest[{dests}]  up[{up}] down[{down}]");
        }

        if (recentEvents.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("Recent:");
            foreach (var message in recentEvents)
                Console.WriteLine("  " + message);
        }
    }

    private static string Arrow(DirectionEnum direction) => direction switch
    {
        DirectionEnum.Up => "^",
        DirectionEnum.Down => "v",
        _ => "-"
    };

    private static string StateGlyph(ElevatorCar car) => car.State switch
    {
        CarStateEnum.Moving => Arrow(car.Direction),
        CarStateEnum.DoorsOpen => "o",
        _ => "=" // Idle
    };
}
