using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;

if (args.Length > 0 && string.Equals(args[0], "settlement", StringComparison.OrdinalIgnoreCase))
{
    RunSettlement(args);
}
else
{
    RunKernelSmoke(args);
}

static void RunKernelSmoke(string[] args)
{
    var seed = args.Length > 0 && ulong.TryParse(args[0], out var parsedSeed)
        ? parsedSeed
        : 42UL;
    var steps = args.Length > 1 && int.TryParse(args[1], out var parsedSteps) && parsedSteps >= 0
        ? parsedSteps
        : 100;

    var simulation = new DeterministicWorldSimulation(new WorldSeed(seed));
    for (var index = 0; index < steps; index++)
    {
        simulation.Advance();
    }

    var json = WorldSnapshotJson.Serialize(simulation.Snapshot);
    Console.WriteLine(json);
    Console.WriteLine($"MWS_HEADLESS_OK seed={seed} steps={steps} tick={simulation.Snapshot.Tick.Value} state={simulation.Snapshot.DeterministicState}");
}

static void RunSettlement(string[] args)
{
    var days = args.Length > 1 && int.TryParse(args[1], out var parsedDays) && parsedDays > 0
        ? Math.Min(parsedDays, 3650)
        : 3;
    var totalHours = checked(days * 24);
    var firstLeg = totalHours / 2;

    var simulation = SettlementSimulation.CreateDefault(new WorldSeed(42));
    simulation.AdvanceHours(firstLeg);

    var firstResident = simulation.Project().Residents[0];
    _ = simulation.InteractWithResident(firstResident.Id, ResidentInteractionChoice.Encourage);
    _ = simulation.GiveItemToResident(firstResident.Id, SettlementItems.Herb, 1);

    var save = SettlementStateJson.Serialize(simulation.CaptureState());
    simulation = SettlementSimulation.Restore(SettlementStateJson.Deserialize(save));
    simulation.AdvanceHours(totalHours - firstLeg);

    var projection = simulation.Project();
    var first = projection.Residents[0];
    Console.WriteLine(SettlementStateJson.Serialize(simulation.CaptureState()));
    Console.WriteLine(
        $"MWS_SETTLEMENT_OK days={days} day={projection.Day} hour={projection.Hour} residents={projection.Residents.Count} " +
        $"pantry={projection.PantryRations} stockpile={projection.Stockpile.Count} workplaces={projection.Workplaces.Count} " +
        $"first_job={first.Profession} first_affinity={first.Affinity} first_inventory={first.Inventory.Count} events={projection.RecentEvents.Count}");
}
