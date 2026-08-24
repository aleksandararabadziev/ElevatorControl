# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy just the project files first so the restore layer is cached
# and only re-runs when a .csproj actually changes.
COPY UI/ElevatorControl.ConsoleApp/*.csproj UI/ElevatorControl.ConsoleApp/
COPY BusinessLayer/ElevatorControl.Domain/*.csproj BusinessLayer/ElevatorControl.Domain/
COPY BusinessLayer/ElevatorControl.Services.Interfaces/*.csproj BusinessLayer/ElevatorControl.Services.Interfaces/
COPY BusinessLayer/ElevatorControl.Services/*.csproj BusinessLayer/ElevatorControl.Services/
RUN dotnet restore UI/ElevatorControl.ConsoleApp/ElevatorControl.ConsoleApp.csproj

# Copy the rest of the source and publish the console app.
COPY BusinessLayer/ BusinessLayer/
COPY UI/ UI/
RUN dotnet publish UI/ElevatorControl.ConsoleApp/ElevatorControl.ConsoleApp.csproj \
    -c Release -o /app --no-restore

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Arguments after the image name are passed through to the app:
#   docker run --rm -it elevatorcontrol [trafficSeconds] [tickMs] [seed]
ENTRYPOINT ["dotnet", "ElevatorControl.ConsoleApp.dll"]
