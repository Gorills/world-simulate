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
    switch (args[index])
    {
        case "--residents" when index + 1 < args.Length && int.TryParse(args[++index], out var parsedResidents):
            residentCount = parsedResidents;
            break;
        case "--days" when index + 1 < args.Length && int.TryParse(args[++index], out var parsedDays):
            days = parsedDays;
            break;
        case "--output" when index + 1 < args.Length:
            outputPath = args[++index];
            break;
        default:
            Console.Error.WriteLine($"Unknown or invalid argument: {args[index]}");
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
var elapsed = Stopwatch.StartNew();
simulation.AdvanceHours(checked((long)days * 24));
elapsed.Stop();

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
    || projection.Residents.Any(resident => resident.Hunger is < 0 or > 100 || resident.Energy is < 0 or > 100))
{
    Console.Error.WriteLine("Settlement scale run violated resident invariants.");
    return 1;
}

var report = new SettlementScaleReport(
    residentCount,
    days,
    checked((long)days * 24),
    elapsed.Elapsed.TotalMilliseconds,
    snapshotWatch.Elapsed.TotalMilliseconds,
    Encoding.UTF8.GetByteCount(snapshot),
    projection.Stockpile.Count,
    projection.Residents.Count,
    projection.Time.Milliseconds);
var json = JsonSerializer.Serialize(report, new JsonSerializerOptions { WriteIndented = true });

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
    $"MWS_SETTLEMENT_SCALE_OK residents={residentCount} days={days} " +
    $"advance_ms={elapsed.Elapsed.TotalMilliseconds:F2} snapshot_bytes={Encoding.UTF8.GetByteCount(snapshot)}");
return 0;

static SettlementState CreateVillageState(int residentCount)
{
    var state = SettlementSimulation.CreateDefault(new WorldSeed(4242), new SimulationScopeId(4242)).CaptureState();
    var farm = state.Workplaces.Single(workplace => workplace.Profession == ResidentProfession.Farmer);
    var kitchen = state.Workplaces.Single(workplace => workplace.Profession == ResidentProfession.Cook);
    var grove = state.Workplaces.Single(workplace => workplace.Profession == ResidentProfession.Forager);
    var residents = Enumerable.Range(1, residentCount)
        .Select(index =>
        {
            var profession = index % 3 switch
            {
                0 => ResidentProfession.Farmer,
                1 => ResidentProfession.Cook,
                _ => ResidentProfession.Forager,
            };
            var workplaceId = profession switch
            {
                ResidentProfession.Farmer => farm.Id,
                ResidentProfession.Cook => kitchen.Id,
                ResidentProfession.Forager => grove.Id,
                _ => throw new InvalidOperationException("Unexpected profession."),
            };
            return new ResidentState(
                new EntityId(index),
                $"Resident {index}",
                10 + (index % 50),
                50 + (index % 51),
                ResidentActivity.Idle,
                profession,
                workplaceId,
                index % 11);
        })
        .ToArray();
    var stacks = state.ItemStacks
        .Select(stack => stack.ItemId switch
        {
            SettlementItems.Ration => stack with { Quantity = Math.Min(int.MaxValue, residentCount * 3) },
            SettlementItems.Grain => stack with { Quantity = Math.Min(int.MaxValue, residentCount * 2) },
            _ => stack,
        })
        .ToArray();

    return state with
    {
        Residents = residents,
        ItemStacks = stacks,
    };
}

internal sealed record SettlementScaleReport(
    int Residents,
    int Days,
    long Hours,
    double AdvanceMilliseconds,
    double SnapshotRoundTripMilliseconds,
    int SnapshotBytes,
    int StockpileStacks,
    int ResidentCountAfter,
    long TimeMilliseconds);
