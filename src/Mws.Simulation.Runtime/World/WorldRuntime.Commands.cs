using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    public SettlementCommandResult ExecuteResidentInteraction(
        SimulationScopeId scopeId,
        EntityId residentId,
        ResidentInteractionChoice choice)
    {
        var commandId = GetLoadedPartition(scopeId).Simulation.NextCommandId;
        return ExecuteSettlementCommand(
            scopeId,
            new InteractWithResidentCommand(commandId, residentId, choice));
    }

    public SettlementCommandResult ExecuteSettlementCommand(
        SimulationScopeId scopeId,
        SettlementCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        EnsureInputJournalCapacity(1);
        var recordedAt = Time;

        var result = ExecuteSettlementCommandCore(scopeId, command);
        var journalCommand = CaptureSettlementCommandInput(scopeId, command);
        RecordInput(CreateInput(
            recordedAt,
            WorldInputKind.SettlementCommand,
            settlementCommand: journalCommand));
        return result;
    }

    private SettlementCommandResult ExecuteSettlementCommandCore(
        SimulationScopeId scopeId,
        SettlementCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        var partition = GetLoadedPartition(scopeId);
        if (command is InteractWithResidentCommand interaction)
        {
            var rejected = ValidateResidentInteraction(
                scopeId,
                partition.Simulation,
                interaction.ResidentId);
            if (rejected is not null)
            {
                return rejected;
            }
        }

        var state = partition.Simulation.CaptureState();
        var staged = SettlementSimulation.Restore(state);
        var result = staged.Execute(command);
        var mutated = command.Id.Value >= state.NextCommandId;

        if (!mutated)
        {
            return result;
        }

        if (partition.Revision == long.MaxValue)
        {
            throw new InvalidOperationException("World partition revision space is exhausted.");
        }

        partition.Simulation = staged;
        partition.Revision = checked(partition.Revision + 1);
        return result;
    }

    private SettlementCommandResult? ValidateResidentInteraction(
        SimulationScopeId scopeId,
        SettlementSimulation simulation,
        EntityId residentId)
    {
        if (_player is null)
        {
            return InteractionRejected(SettlementResultCodes.PlayerRequired, residentId);
        }

        if (_player.ScopeId != scopeId)
        {
            return InteractionRejected(SettlementResultCodes.PlayerScopeMismatch, residentId);
        }

        var residentLocation = simulation.TryGetResidentSemanticLocation(residentId);
        if (residentLocation is null)
        {
            return null;
        }

        var playerLocation = SettlementSemanticLocation.Normalize(_player.Location);
        if (playerLocation.Kind == SettlementActorLocationKind.Travelling
            || residentLocation.Kind == SettlementActorLocationKind.Travelling)
        {
            return InteractionRejected(SettlementResultCodes.InteractionActorTravelling, residentId);
        }

        if (playerLocation.CurrentPlace != SettlementPlaceRef.Settlement
            && playerLocation.CurrentPlace != residentLocation.CurrentPlace)
        {
            return InteractionRejected(SettlementResultCodes.InteractionNotCoLocated, residentId);
        }

        return null;
    }

    private static SettlementCommandResult InteractionRejected(string code, EntityId residentId) =>
        new(false, code, residentId, []);

    private static WorldSettlementCommandInput CaptureSettlementCommandInput(
        SimulationScopeId scopeId,
        SettlementCommand command) =>
        command switch
        {
            FeedResidentCommand feed => new WorldSettlementCommandInput(
                scopeId,
                WorldSettlementCommandKind.FeedResident,
                feed.Id,
                feed.ResidentId,
                null,
                0,
                null),
            GiveItemToResidentCommand give => new WorldSettlementCommandInput(
                scopeId,
                WorldSettlementCommandKind.GiveItemToResident,
                give.Id,
                give.ResidentId,
                give.ItemId,
                give.Quantity,
                null),
            InteractWithResidentCommand interact => new WorldSettlementCommandInput(
                scopeId,
                WorldSettlementCommandKind.InteractWithResident,
                interact.Id,
                interact.ResidentId,
                null,
                0,
                interact.Choice),
            _ => throw new ArgumentOutOfRangeException(
                nameof(command),
                command.GetType().Name,
                "Unknown settlement command."),
        };
}
