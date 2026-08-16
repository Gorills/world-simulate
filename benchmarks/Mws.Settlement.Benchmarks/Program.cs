using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;

var residentCount = 512;
var days = 30;
string? outputPath = null;

for (var index = 0; index < args.Length; index++)
{
    if (string.Equals(args[index], "--residents", StringComparison.Ordinal))
    {
        if (index + 1 >= args.Length || !int.TryParse(args[++index], out residentCount))
        {
            Console.Error.WriteLine("--residents requires an integer value.");
            return 2;
        }
    }
    else if (string.Equals(args[index], "--days", StringComparison.Ordinal))
    {
        if (index + 1 >= args.Length || !int.TryParse(args[++index], out days))
        {
            Console.Error.WriteLine("--days requires an integer value.");
            return 2;
        }
    }
    else if (string.Equals(args[index], "--output", StringComparison.Ordinal))
    {
        if (index + 1 >= args.Length)
        {
            Console.Error.WriteLine("--output requires a file path.");
            return 2;
        }

        outputPath = args[++index];
    }
    else
    {
        Console.Error.WriteLine($"Unknown argument: {args[index]}");
        return 2;
    }
}

if (residentCount is < 3 or > 5_000 || days is < 1 or > 365)
{
    Console.Error.WriteLine("Residents must be 3..5000 and days must be 1..365.");
    return 2;
}

var state = CreateVillageState(residentCount);
var simulation = SettlementSimulation.Restore(state);
var allocatedBefore = GC.GetTotalAllocatedBytes(precise: true);
var elapsed = Stopwatch.StartNew();
simulation.AdvanceHours(checked((long)days * 24));
elapsed.Stop();
var advanceAllocatedBytes = checked(GC.GetTotalAllocatedBytes(precise: true) - allocatedBefore);

var snapshotWatch = Stopwatch.StartNew();
var snapshot = SettlementStateJson.Serialize(simulation.CaptureState());
var restored = SettlementSimulation.Restore(SettlementStateJson.Deserialize(snapshot));
var roundTrip = SettlementStateJson.Serialize(restored.CaptureState());
snapshotWatch.Stop();

if (!string.Equals(snapshot, roundTrip, StringComparison.Ordinal))
{
    Console.Error.WriteLine("Settlement scale snapshot round-trip diverged.");
    return 1;
}

var projection = restored.Project();
if (projection.Residents.Count != residentCount
    || projection.Workplaces.Count != residentCount
    || projection.Residents.Any(resident => resident.Hunger is < 0 or > 100 || resident.Energy is < 0 or > 100))
{
    Console.Error.WriteLine("Settlement scale run violated resident or topology invariants.");
    return 1;
}

var snapshotBytes = Encoding.UTF8.GetByteCount(snapshot);
var report = new SettlementScaleReport(
    residentCount,
    projection.Workplaces.Count,
    days,
    checked((long)days * 24),
    elapsed.Elapsed.TotalMilliseconds,
    advanceAllocatedBytes,
    snapshotWatch.Elapsed.TotalMilliseconds,
    snapshotBytes,
    projection.Stockpile.Count,
    projection.Residents.Count,
    projection.Time.Milliseconds);
var json = JsonSerializer.Serialize(report);

if (outputPath is not null)
{
    var directory = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }

    File.WriteAllText(outputPath, json, Encoding.UTF8);
}

Console.WriteLine(json);
Console.WriteLine(
    $"MWS_SETTLEMENT_SCALE_OK residents={residentCount} days={days} workplaces={projection.Workplaces.Count} " +
    $"advance_ms={elapsed.Elapsed.TotalMilliseconds:F2} allocated_bytes={advanceAllocatedBytes} snapshot_bytes={snapshotBytes}");
return 0;

static SettlementState CreateVillageState(int residentCount)
{
    var state = SettlementSimulation.CreateDefault(new WorldSeed(4242), new SimulationScopeId(4242)).CaptureState();
    var workplaces = Enumerable.Range(1, residentCount)
        .Select(index =>
        {
            var profession = ProfessionFor(index);
            var workplaceId = new EntityId(2_000_000L + index);
            return profession switch
            {
                ResidentProfession.Farmer => new WorkplaceState(
                    workplaceId,
                    $"Farm {index}",
                    profession,
                    null,
                    0,
                    SettlementItems.Grain,
                    2),
                ResidentProfession.Cook => new WorkplaceState(
                    workplaceId,
                    $"Kitchen {index}",
                    profession,
                    SettlementItems.Grain,
                    2,
                    SettlementItems.Ration,
                    1),
                ResidentProfession.Forager => new WorkplaceState(
                    workplaceId,
                    $"Grove {index}",
                    profession,
                    null,
                    0,
                    SettlementItems.Herb,
                    1),
                _ => throw new InvalidOperationException("Unexpected profession."),
            };
        })
        .ToArray();
    var residents = Enumerable.Range(1, residentCount)
        .Select(index =>
        {
            var profession = ProfessionFor(index);
            return new ResidentState(
                new EntityId(1_000_000L + index),
                $"Resident {index}",
                10 + (index % 50),
                50 + (index % 51),
                ResidentActivity.Idle,
                profession,
                workplaces[index - 1].Id,
                index % 11);
        })
        .ToArray();
    var stacks = state.ItemStacks
        .Select(stack => stack.ItemId switch
        {
            SettlementItems.Ration => stack with { Quantity = residentCount * 3 },
            SettlementItems.Grain => stack with { Quantity = residentCount * 2 },
            _ => stack,
        })
        .ToArray();

    return state with
    {
        Residents = residents,
        ItemStacks = stacks,
        Workplaces = workplaces,
    };
}

static ResidentProfession ProfessionFor(int index) => (index % 3) switch
{
    0 => ResidentProfession.Farmer,
    1 => ResidentProfession.Cook,
    _ => ResidentProfession.Forager,
};

internal sealed record SettlementScaleReport(
    int Residents,
    int Workplaces,
    int Days,
    long Hours,
    double AdvanceMilliseconds,
    long AdvanceAllocatedBytes,
    double SnapshotRoundTripMilliseconds,
    int SnapshotBytes,
    int StockpileStacks,
    int ResidentCountAfter,
    long TimeMilliseconds);
