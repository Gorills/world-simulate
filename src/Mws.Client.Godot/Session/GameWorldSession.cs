using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;

namespace Mws.Client.Godot.Session;

internal sealed class GameWorldSession
{
    private readonly WorldRuntime _world;
    private readonly SimulationScopeId _settlementScopeId;
    private readonly EntityId _playerId;
    private bool _travelPlaytestPending;

    public GameWorldSession(WorldSeed seed)
    {
        var bootstrap = P3TravelPlaytestScenario.Create(seed);
        _world = bootstrap.World;
        _settlementScopeId = bootstrap.ScopeId;
        _playerId = bootstrap.PlayerId;
        SelectedResidentId = bootstrap.ResidentId;
        _travelPlaytestPending = true;
    }

    private GameWorldSession(WorldRuntime world)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        _playerId = _world.PlayerId
            ?? throw new InvalidOperationException("Playable world checkpoint has no authoritative player actor.");
        var player = _world.ProjectPlayer();
        _settlementScopeId = player.ScopeId;
        var projection = _world.ProjectSettlement(_settlementScopeId);
        if (projection.Residents.Count == 0)
        {
            throw new InvalidOperationException("Playable settlement must contain at least one resident.");
        }

        SelectedResidentId = projection.Residents[0].Id;
        _travelPlaytestPending = false;
    }

    public event Action? Changed;

    public EntityId SelectedResidentId { get; private set; }

    public EntityId PlayerId => _playerId;

    public SimulationScopeId SettlementScopeId => _settlementScopeId;

    public SimulationTime Time => _world.Time;

    public WorldPlayerProjection Player => _world.ProjectPlayer();

    public SettlementProjection Projection => _world.ProjectSettlement(_settlementScopeId);

    public ResidentProjection SelectedResident =>
        Projection.Residents.Single(resident => resident.Id == SelectedResidentId);

    internal bool TravelPlaytestPending => _travelPlaytestPending;

    public static GameWorldSession Restore(WorldCheckpointState checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        return new GameWorldSession(WorldRuntime.Restore(checkpoint));
    }

    public WorldCheckpointState CreateCheckpoint() => _world.CreateCheckpoint();

    public void SelectResident(EntityId residentId)
    {
        if (!Projection.Residents.Any(resident => resident.Id == residentId))
        {
            return;
        }

        SelectedResidentId = residentId;
        Changed?.Invoke();
    }

    public void SelectRelative(int offset)
    {
        var residents = Projection.Residents;
        var currentIndex = residents
            .Select((resident, index) => (resident, index))
            .First(entry => entry.resident.Id == SelectedResidentId)
            .index;
        var nextIndex = (currentIndex + offset) % residents.Count;
        if (nextIndex < 0)
        {
            nextIndex += residents.Count;
        }

        SelectedResidentId = residents[nextIndex].Id;
        Changed?.Invoke();
    }

    public ItemStackProjection? FindStockpileStack(long stackId) =>
        Projection.Stockpile.SingleOrDefault(stack => stack.StackId == stackId);

    public SettlementCommandResult InteractSelected(ResidentInteractionChoice choice)
    {
        var result = _world.ExecuteResidentInteraction(_settlementScopeId, SelectedResidentId, choice);
        Changed?.Invoke();
        return result;
    }

    public void AdvanceHours(int hours)
    {
        _world.AdvanceHours(hours);
        _travelPlaytestPending = false;
        Changed?.Invoke();
    }

    internal bool TryStartTravelPlaytest()
    {
        if (!_travelPlaytestPending)
        {
            return false;
        }

        var before = SelectedResident;
        var location = before.Location;
        var requiredPlace = before.SelectedTask?.RequiredPlace;
        if (location is null
            || requiredPlace is null
            || location.Kind != SettlementActorLocationKind.AtPlace
            || location.CurrentPlace == requiredPlace.Value)
        {
            return false;
        }

        var remainder = Time.Milliseconds % SettlementSimulation.HourMilliseconds;
        var toNextDecisionBoundary = remainder == 0
            ? SettlementSimulation.HourMilliseconds
            : SettlementSimulation.HourMilliseconds - remainder;
        _world.AdvanceTo(new SimulationTime(checked(Time.Milliseconds + toNextDecisionBoundary)));
        var started = SelectedResident.Location?.Kind == SettlementActorLocationKind.Travelling;
        if (started)
        {
            _travelPlaytestPending = false;
        }

        Changed?.Invoke();
        return started;
    }

    internal bool AdvanceActiveTravelSample(long elapsedMilliseconds)
    {
        if (elapsedMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elapsedMilliseconds),
                elapsedMilliseconds,
                "Travel sample duration must be positive.");
        }

        _world.AdvanceTo(new SimulationTime(checked(Time.Milliseconds + elapsedMilliseconds)));
        _travelPlaytestPending = false;
        var active = HasActiveTravel();
        Changed?.Invoke();
        return active;
    }

    private bool HasActiveTravel()
    {
        if (Player.Location?.Kind == SettlementActorLocationKind.Travelling)
        {
            return true;
        }

        return Projection.Residents.Any(
            resident => resident.Location?.Kind == SettlementActorLocationKind.Travelling);
    }
}
