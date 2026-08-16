using System.Globalization;
using System.Text.Json;

var outputDirectory = args.Length > 0 ? args[0] : "artifacts/benchmarks";
var iterations = args.Length > 1
    && int.TryParse(args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedIterations)
        ? Math.Max(parsedIterations, 1_000)
        : 20_000;
var repetitions = args.Length > 2
    && int.TryParse(args[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedRepetitions)
        ? Math.Max(parsedRepetitions, 2)
        : 3;

Directory.CreateDirectory(outputDirectory);
var results = new[]
{
    ProofABenchmarks.RunKernelEvents(iterations, repetitions),
    ProofABenchmarks.RunSaveReplay(Math.Max(iterations / 10, 1_000), repetitions),
    ProofABenchmarks.RunTrace(Math.Max(iterations / 5, 1_000), repetitions),
    ProofABenchmarks.RunLod(Math.Max(iterations / 100, 500), repetitions),
};

foreach (var result in results)
{
    var path = Path.Combine(outputDirectory, $"{result.WorkloadId}_v1.json");
    File.WriteAllText(path, JsonSerializer.Serialize(result, BenchmarkSupport.JsonOptions));
    Console.WriteLine($"MWS_BENCHMARK_OK workload={result.WorkloadId} result={path}");
}
