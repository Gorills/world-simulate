using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;

var label = "subject";
var partitions = 8;
var advanceHours = 48;
var commands = 128;
var samples = 5;
string? outputPath = null;

for (var index = 0; index < args.Length; index++)
{
    switch (args[index])
    {
        case "--label":
            label = RequireValue(args, ref index, "--label");
            break;
        case "--partitions":
            partitions = ParseInt(args, ref index, "--partitions");
            break;
        case "--advance-hours":
            advanceHours = ParseInt(args, ref index, "--advance-hours");
            break;
        case "--commands":
            commands = ParseInt(args, ref index, "--commands");
            break;
        case "--samples":
            samples = ParseInt(args, ref index, "--samples");
            break;
        case "--output":
            outputPath = RequireValue(args, ref index, "--output");
            break;
        default:
            Console.Error.WriteLine($"Unknown argument: {args[index]}");
            return 2;
    }
}

if (string.IsNullOrWhiteSpace(label)
    || partitions is < 1 or > 64
    || advanceHours is < 1 or > 720
    || commands is < 1 or > 2_048
    || samples is < 3 or > 11
    || (samples & 1) == 0)
{
    Console.Error.WriteLine(
        "Expected non-empty label, partitions 1..64, advance-hours 1..720, commands 1..2048, " +
        "and an odd sample count 3..11.");
    return 2;
}

WarmUp();

var advanceSamples = new Measurement[samples];
var commandSamples = new Measurement[samples];
for (var sample = 0; sample < samples; sample++)
{
    advanceSamples[sample] = MeasureAdvance(partitions, advanceHours);
    commandSamples[sample] = MeasureCommands(commands);
}

var report = new WorldHotPathReport(
    label,
    Environment.Version.ToString(),
    new WorldHotPathScenario(partitions, advanceHours, commands, samples),
    Summarize(advanceSamples),
    Summarize(commandSamples));
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
    $"MWS_P4_WORLD_HOT_PATH_OK label={label} " +
    $"advance_ms={report.Advance.MedianMilliseconds:F4} " +
    $"advance_allocated_bytes={report.Advance.MedianAllocatedBytes} " +
    $"command_ms={report.Commands.MedianMilliseconds:F4} " +
    $"command_allocated_bytes={report.Commands.MedianAllocatedBytes}");
return 0;

static void WarmUp()
{
    var world = WorldRuntime.Create(new WorldSeed(9499));
    var first = world.AddDefaultSettlement();
    _ = world.AddDefaultSettlement();
    world.AdvanceHours(2);
    var state = world.CaptureSettlementState(first);
    var residentId = state.Residents[0].Id;
    for (var index = 0; index < 8; index++)
    {
        _ = world.ExecuteSettlementCommand(
            first,
            new FeedResidentCommand(new CommandId(state.NextCommandId + index), residentId));
    }
}

static Measurement MeasureAdvance(int partitions, int hours)
{
    var world = WorldRuntime.Create(new WorldSeed(9501));
    for (var index = 0; index < partitions; index++)
    {
        _ = world.AddDefaultSettlement();
    }

    Collect();
    var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
    var stopwatch = Stopwatch.StartNew();
    for (var hour = 0; hour < hours; hour++)
    {
        world.AdvanceHours(1);
    }

    stopwatch.Stop();
    var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
    var expected = checked((long)hours * SettlementSimulation.HourMilliseconds);
    if (world.Time.Milliseconds != expected)
    {
        throw new InvalidOperationException("World advance benchmark did not reach the expected time.");
    }

    return new Measurement(stopwatch.Elapsed.TotalMilliseconds, allocated);
}

static Measurement MeasureCommands(int commands)
{
    var world = WorldRuntime.Create(new WorldSeed(9502));
    var scope = world.AddDefaultSettlement();
    var state = world.CaptureSettlementState(scope);
    var residentId = state.Residents[0].Id;
    var firstCommandId = state.NextCommandId;

    Collect();
    var allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
    var stopwatch = Stopwatch.StartNew();
    for (var index = 0; index < commands; index++)
    {
        _ = world.ExecuteSettlementCommand(
            scope,
            new FeedResidentCommand(new CommandId(firstCommandId + index), residentId));
    }

    stopwatch.Stop();
    var allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
    var after = world.CaptureSettlementState(scope);
    if (after.NextCommandId != firstCommandId + commands)
    {
        throw new InvalidOperationException("World command benchmark did not consume the expected fresh command IDs.");
    }

    return new Measurement(stopwatch.Elapsed.TotalMilliseconds, allocated);
}

static MeasurementSummary Summarize(Measurement[] measurements)
{
    var elapsed = measurements.Select(entry => entry.ElapsedMilliseconds).OrderBy(value => value).ToArray();
    var allocated = measurements.Select(entry => entry.AllocatedBytes).OrderBy(value => value).ToArray();
    var middle = measurements.Length / 2;
    return new MeasurementSummary(
        elapsed[middle],
        allocated[middle],
        measurements);
}

static void Collect()
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
}

static int ParseInt(string[] args, ref int index, string option)
{
    var value = RequireValue(args, ref index, option);
    if (!int.TryParse(value, out var parsed))
    {
        throw new ArgumentException($"{option} requires an integer value.");
    }

    return parsed;
}

static string RequireValue(string[] args, ref int index, string option)
{
    if (index + 1 >= args.Length)
    {
        throw new ArgumentException($"{option} requires a value.");
    }

    return args[++index];
}

internal sealed record WorldHotPathReport(
    string Label,
    string RuntimeVersion,
    WorldHotPathScenario Scenario,
    MeasurementSummary Advance,
    MeasurementSummary Commands);

internal sealed record WorldHotPathScenario(
    int Partitions,
    int AdvanceHours,
    int Commands,
    int Samples);

internal sealed record MeasurementSummary(
    double MedianMilliseconds,
    long MedianAllocatedBytes,
    Measurement[] Samples);

internal readonly record struct Measurement(
    double ElapsedMilliseconds,
    long AllocatedBytes);
