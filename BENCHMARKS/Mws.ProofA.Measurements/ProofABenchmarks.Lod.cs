using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;

internal static partial class ProofABenchmarks
{
    public static BenchmarkResult RunLod(int iterations, int repetitions)
    {
        var aggregates = new List<double>(repetitions);
        var materializations = new List<double>(repetitions);
        var identityMismatches = 0;
        var conservationMismatches = 0;
        var informationMismatches = 0;
        long stateBytes = 0;

        for (var repetition = 0; repetition < repetitions; repetition++)
        {
            var kernel = new ProofAKernel(new WorldSeed(45));
            var owner = kernel.CreateEntity();
            var ids = new List<EntityId>();
            for (var index = 0; index < 64; index++)
            {
                ids.Add(kernel.CreateEntity(owner, initialResource: index + 1, rare: index == 63));
            }
            var processId = kernel.StartLongProcess(new CommandId(1), owner, ids[^1], 10_000, 3);
            ProofARegionAggregate? aggregate = null;

            var aggregateWatch = Stopwatch.StartNew();
            for (var index = 0; index < iterations; index++)
            {
                aggregate = kernel.AggregateRegion(ids);
            }
            aggregateWatch.Stop();
            IReadOnlyList<ProofALodMember> members = [];
            var materializeWatch = Stopwatch.StartNew();
            for (var index = 0; index < iterations; index++)
            {
                members = ProofAKernel.MaterializeRegion(aggregate!);
            }
            materializeWatch.Stop();

            var expectedIds = ids.Select(id => id.Value).OrderBy(value => value).ToArray();
            var actualIds = members.Select(member => member.Id.Value).OrderBy(value => value).ToArray();
            if (!expectedIds.SequenceEqual(actualIds))
            {
                identityMismatches++;
            }
            var expectedTotal = ids.Sum(id =>
            {
                _ = kernel.TryGetEntity(id, out var entity);
                return entity!.Resource;
            });
            if (aggregate!.TotalResource != expectedTotal || members.Sum(member => member.Resource) != expectedTotal)
            {
                conservationMismatches++;
            }
            if (!aggregate.RareEntityIds.Contains(ids[^1]) || !aggregate.PendingProcessIds.Contains(processId))
            {
                informationMismatches++;
            }
            aggregates.Add(aggregateWatch.Elapsed.TotalMilliseconds / iterations);
            materializations.Add(materializeWatch.Elapsed.TotalMilliseconds / iterations);
            stateBytes = Math.Max(stateBytes, Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(aggregate, BenchmarkSupport.JsonOptions)));
        }

        return BenchmarkSupport.Create(
            "RW-D_LOD_ROUNDTRIP",
            iterations,
            repetitions,
            new Dictionary<string, object>
            {
                ["materialize_latency"] = materializations.Average(),
                ["aggregate_latency"] = aggregates.Average(),
                ["state_bytes"] = stateBytes,
                ["identity_mismatch_count"] = identityMismatches,
                ["conservation_mismatch_count"] = conservationMismatches,
                ["information_mismatch_count"] = informationMismatches,
            },
            "Micro/aggregate/materialize preserves stable IDs, conserved resource, rare-tail identity, and pending-process references.");
    }
}
