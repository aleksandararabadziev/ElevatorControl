using ElevatorControl.ConsoleApp;
using ElevatorControl.Domain;
using ElevatorControl.Domain.Enums;
using ElevatorControl.Services;
using ElevatorControl.Services.Interfaces;

// Optional args: [trafficSeconds] [tickMs] [seed]
//   trafficSeconds - how long new calls are generated for (default 20)
//   tickMs         - real time per simulated second, 0 = as fast as possible (default 1000)
//   seed           - fixes the random traffic so a run is reproducible (default random)
const int DefaultTrafficSeconds = 20;
const int DefaultTickMs = 1000;
const int MaxSeconds = 300;      // safety cap so the sim can never run forever
const int MaxRecentEvents = 8;

var trafficSeconds = ArgOr(args, 0, DefaultTrafficSeconds);
var tickMs = ArgOr(args, 1, DefaultTickMs);
int? seed = args.Length > 2 && int.TryParse(args[2], out var s) ? s : null;

var config = new SimulationConfig
{
    NumberOfFloors = 5,
    NumberOfCars = 2,
    SecondsPerFloor = 1,
    SecondsPerStop = 2
};

// Keep the last few events so the dashboard can show a running commentary.
var recentEvents = new List<string>();
config.Log = message =>
{
    recentEvents.Add(message);
    if (recentEvents.Count > MaxRecentEvents)
        recentEvents.RemoveAt(0);
};

var scheduler = new Scheduler();
var dispatcher = new Dispatcher(config);
ITrafficService traffic = new TimeLimitedTrafficService(
    new TrafficService(config, seed is null ? null : new Random(seed.Value)),
    trafficSeconds);
var engine = new SimulationEngine(config, scheduler, dispatcher, traffic);

Console.WriteLine($"ElevatorControl - {config.NumberOfCars} cars, {config.NumberOfFloors} floors.");
Console.WriteLine($"Generating traffic for {trafficSeconds}s, then draining. {tickMs}ms per second.");
Console.WriteLine();

while (true)
{
    engine.Step();
    ConsoleRenderer.Render(engine, config, trafficSeconds, recentEvents);

    if (IsFinished(engine, trafficSeconds))
    {
        Console.WriteLine();
        Console.WriteLine($"Done - everyone delivered after {engine.ElapsedSeconds}s.");
        break;
    }

    if (engine.ElapsedSeconds >= MaxSeconds)
    {
        Console.WriteLine();
        Console.WriteLine($"Stopped at the {MaxSeconds}s safety limit.");
        break;
    }

    if (tickMs > 0)
        Thread.Sleep(tickMs);
}

// The run is over once traffic has stopped and the building is empty and quiet.
static bool IsFinished(ISimulationEngine engine, int trafficSeconds)
{
    var trafficDone = engine.ElapsedSeconds >= trafficSeconds;
    var nobodyWaiting = engine.WaitingPassengers.Count == 0;
    var carsEmptyAndIdle = engine.Cars.All(c =>
        c.Passengers.Count == 0 && c.State == CarStateEnum.Idle);

    return trafficDone && nobodyWaiting && carsEmptyAndIdle;
}

static int ArgOr(string[] args, int index, int fallback)
    => args.Length > index && int.TryParse(args[index], out var value) ? value : fallback;
