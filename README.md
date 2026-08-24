# ElevatorControl

A small .NET 8 console simulation of a few elevator cars moving people around a
building. Passengers appear at random over time, calls are dispatched to the best
available car, and each car serves its stops using the **LOOK** scheduling rule.
The simulation runs in real time so you can watch it in the console.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Restoring the test project pulls xUnit from NuGet, so an internet connection is
  needed the first time you build.

## Build

From the solution root:

```bash
dotnet build
```

## Run

```bash
dotnet run --project UI/ElevatorControl.ConsoleApp
```

By default it generates traffic for 20 seconds, then keeps running until every
passenger has been delivered, advancing one simulated second per real second.

### Command-line arguments

All optional and positional: `[trafficSeconds] [tickMs] [seed]`

| Argument         | Meaning                                                             | Default  |
|------------------|--------------------------------------------------------------------|----------|
| `trafficSeconds` | How long new calls are generated for.                              | `20`     |
| `tickMs`         | Real milliseconds per simulated second. `0` runs as fast as possible. | `1000`   |
| `seed`           | Fixes the random traffic so a run is reproducible.                 | random   |

Examples:

```bash
# Watch a longer run at half speed
dotnet run --project UI/ElevatorControl.ConsoleApp -- 30 500

# Reproduce a specific run instantly (seed 42, no delay)
dotnet run --project UI/ElevatorControl.ConsoleApp -- 20 0 42
```

### Reading the dashboard

The screen is redrawn each tick:

```
ElevatorControl   t=6s   traffic:ON (14s left)   waiting:2
--------------------------------------------------------------
F 5 |                 |
F 4 |                 |
F 3 | [C1^]           | wait: P4v
F 2 | [C2=]           |
F 1 |                 | wait: P1^

Cars:
  C1  floor 3  Moving    ^  pax:1 dest[4]  up[4] down[3]
  C2  floor 2  Idle      -  pax:0 dest[-]  up[-] down[-]

Recent:
  Passenger 4 calls at floor 3 going Down (wants floor 2).
  ...
```

- The building is drawn top floor first. `[C1^]` is car 1 and its glyph:
  `^`/`v` = moving up/down, `o` = doors open, `=` = idle.
- `wait: P4v` means passenger 4 is waiting on that floor to go down.
- The `Cars:` block lists each car's floor, state, heading, onboard passenger
  destinations (`dest[...]`), and pending stops (`up[...]` / `down[...]`).
- `Recent:` shows the last few events.

## Run in a container

A multi-stage `Dockerfile` at the repository root builds and publishes the
console app on top of the .NET 8 runtime image.

Build the image from the solution root:

```bash
docker build -t elevatorcontrol .
```

Run it. Use `-it` so the live dashboard (which redraws the screen each tick)
renders correctly and so `Ctrl+C` stops it:

```bash
docker run --rm -it elevatorcontrol
```

The same optional arguments are passed straight through after the image name:

```bash
# 30s of traffic, half speed, reproducible (seed 42)
docker run --rm -it elevatorcontrol 30 500 42
```

Without a TTY (for example in CI, or `docker run` without `-t`) the app detects
that output is redirected and prints each frame on a new line instead of
clearing the screen, so it still works when captured to logs.

## Configuration

The building and timing are set in `SimulationConfig` (in the `Domain` project).
The console app fills it in near the top of `UI/ElevatorControl.ConsoleApp/Program.cs`:

| Property          | Meaning                                             | Default |
|-------------------|-----------------------------------------------------|---------|
| `NumberOfFloors`  | Floors in the building.                             | `5`     |
| `NumberOfCars`    | Elevator cars.                                      | `2`     |
| `SecondsPerFloor` | Simulated seconds to travel one floor (>= 1).       | `1`     |
| `SecondsPerStop`  | Simulated seconds the doors stay open at a stop (>= 1). | `2`  |
| `Log`             | Optional `Action<string>` the engine calls to report events. | none |

To change the building, edit the `new SimulationConfig { ... }` in `Program.cs`.
A couple of other knobs live nearby:

- `Program.cs` — `DefaultTrafficSeconds`, `DefaultTickMs`, `MaxSeconds` (a safety
  cap so a run can never loop forever), and `MaxRecentEvents` (how many events the
  dashboard shows).
- `TrafficService.cs` — `SpawnChance` (probability a new passenger appears each tick).

## Tests

```bash
dotnet test
```

With coverage (coverlet is already referenced by the test project):

```bash
dotnet test --collect:"XPlat Code Coverage"
```

The tests cover the domain types and every service (`Scheduler`, `Dispatcher`,
`TrafficService`, `SimulationEngine`), including the LOOK edge cases and the
dispatcher's scoring tiers.

## Project structure

A layered solution; each layer only depends on the ones beneath it.

```
ElevatorControl.sln
├─ Dockerfile                                container build (publishes the console app)
├─ .dockerignore
├─ BusinessLayer/
│  ├─ ElevatorControl.Domain              types only (no dependencies)
│  ├─ ElevatorControl.Services.Interfaces service contracts (-> Domain)
│  └─ ElevatorControl.Services            service implementations (-> Interfaces, Domain)
├─ UI/
│  └─ ElevatorControl.ConsoleApp          composition root + live dashboard
└─ Tests/
   └─ ElevatorControl.Tests               xUnit tests (-> Domain, Interfaces, Services)
```

### Domain

- `ElevatorCar` — a car's live state: current floor, direction, `State`
  (`Idle`/`Moving`/`DoorsOpen`), its pending `UpStops` / `DownStops`
  (sorted sets), and the passengers aboard.
- `Passenger` — origin floor (where the call was made), destination, and a
  derived travel `Direction`.
- `SimulationConfig` — the building setup and timing described above.
- `Enums/DirectionEnum` (`Down = -1`, `None = 0`, `Up = 1`, so it doubles as
  step arithmetic) and `Enums/CarStateEnum`.

### Services

- `IScheduler` / `Scheduler` — decides how a single car moves under **LOOK**:
  it keeps going in its current direction, stopping for same-direction calls,
  and only reverses once nothing remains ahead in that direction. Everything
  (which way to face, whether to stop) is derived from one "next target"
  calculation.
- `IDispatcher` / `Dispatcher` — assigns a hall call to the best car. Idle cars
  and cars already approaching the call score best (by distance); a car that must
  finish its sweep first is penalised, and a car heading the wrong way is
  penalised the most.
- `ITrafficService` / `TrafficService` — randomly creates passengers over time.
  Each passenger is both a call (origin + direction) and a stop (destination).
  Accepts an optional `Random` so runs can be seeded.
- `ISimulationEngine` / `SimulationEngine` — the core loop. Each `Step()`
  (one simulated second) creates traffic, dispatches new calls, and advances the
  cars, keeping their state current. A car only picks up passengers travelling
  its current direction. Live state is exposed via `Cars`, `WaitingPassengers`,
  and `ElapsedSeconds`.

### UI

- `Program.cs` — wires the services together, runs the real-time loop with the
  traffic time-limit, and stops once the building is empty (or the safety cap).
- `TimeLimitedTrafficService.cs` — wraps the traffic service so calls are only
  generated during the traffic window, letting the simulation drain afterwards.
- `ConsoleRenderer.cs` — draws the dashboard shown above.
