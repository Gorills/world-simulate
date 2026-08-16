using System.Globalization;
using System.Text;
using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Persistence.Json;

public sealed class JsonWorldStore
{
    private const string CurrentFileName = "CURRENT";
    private const string ManifestFileName = "manifest.json";
    private readonly string _rootPath;

    public JsonWorldStore(string rootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);
        _rootPath = Path.GetFullPath(rootPath);
    }

    public void SaveCheckpoint(WorldCheckpointState checkpoint)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        ValidateCheckpoint(checkpoint);
        Directory.CreateDirectory(_rootPath);

        var checkpointId = checkpoint.Manifest.CheckpointId;
        var finalPath = CheckpointPath(checkpointId);
        var stagingPath = finalPath + ".tmp";
        if (Directory.Exists(finalPath))
        {
            throw new IOException($"World checkpoint {checkpointId} already exists.");
        }

        if (Directory.Exists(stagingPath))
        {
            Directory.Delete(stagingPath, recursive: true);
        }

        var partitionsPath = Path.Combine(stagingPath, "partitions");
        Directory.CreateDirectory(partitionsPath);
        try
        {
            foreach (var partition in checkpoint.Partitions.OrderBy(entry => entry.ScopeId.Value))
            {
                File.WriteAllText(
                    PartitionPath(stagingPath, partition.ScopeId),
                    SettlementStateJson.Serialize(partition.Settlement),
                    Encoding.UTF8);
            }

            File.WriteAllText(
                Path.Combine(stagingPath, ManifestFileName),
                WorldManifestJson.Serialize(checkpoint.Manifest),
                Encoding.UTF8);
            Directory.Move(stagingPath, finalPath);
            WriteCurrentCheckpointId(checkpointId);
        }
        catch
        {
            if (Directory.Exists(stagingPath))
            {
                Directory.Delete(stagingPath, recursive: true);
            }

            throw;
        }
    }

    public WorldManifestState LoadManifest()
    {
        var checkpointPath = CheckpointPath(ReadCurrentCheckpointId());
        var manifestPath = Path.Combine(checkpointPath, ManifestFileName);
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException("World manifest is missing from the current checkpoint.", manifestPath);
        }

        return WorldManifestJson.Deserialize(File.ReadAllText(manifestPath, Encoding.UTF8));
    }

    public SettlementState LoadSettlement(SimulationScopeId scopeId)
    {
        var checkpointId = ReadCurrentCheckpointId();
        var checkpointPath = CheckpointPath(checkpointId);
        var manifest = LoadManifestFromCheckpoint(checkpointPath);
        if (manifest.Partitions.All(entry => entry.ScopeId != scopeId))
        {
            throw new KeyNotFoundException($"Settlement scope {scopeId.Value} is not present in checkpoint {checkpointId}.");
        }

        return LoadSettlementFromCheckpoint(checkpointPath, scopeId);
    }

    public WorldCheckpointState LoadCheckpoint()
    {
        var checkpointPath = CheckpointPath(ReadCurrentCheckpointId());
        var manifest = LoadManifestFromCheckpoint(checkpointPath);
        var partitions = manifest.Partitions
            .OrderBy(entry => entry.ScopeId.Value)
            .Select(descriptor => new WorldPartitionState(
                descriptor.ScopeId,
                descriptor.Revision,
                LoadSettlementFromCheckpoint(checkpointPath, descriptor.ScopeId)))
            .ToArray();
        return new WorldCheckpointState(manifest, partitions);
    }

    private static WorldManifestState LoadManifestFromCheckpoint(string checkpointPath)
    {
        var manifestPath = Path.Combine(checkpointPath, ManifestFileName);
        if (!File.Exists(manifestPath))
        {
            throw new FileNotFoundException("World manifest is missing from the current checkpoint.", manifestPath);
        }

        return WorldManifestJson.Deserialize(File.ReadAllText(manifestPath, Encoding.UTF8));
    }

    private static SettlementState LoadSettlementFromCheckpoint(
        string checkpointPath,
        SimulationScopeId scopeId)
    {
        var partitionPath = PartitionPath(checkpointPath, scopeId);
        if (!File.Exists(partitionPath))
        {
            throw new FileNotFoundException($"Settlement partition {scopeId.Value} is missing.", partitionPath);
        }

        return SettlementStateJson.Deserialize(File.ReadAllText(partitionPath, Encoding.UTF8));
    }

    private void WriteCurrentCheckpointId(long checkpointId)
    {
        var currentPath = Path.Combine(_rootPath, CurrentFileName);
        var temporaryPath = currentPath + ".tmp";
        File.WriteAllText(temporaryPath, checkpointId.ToString(CultureInfo.InvariantCulture), Encoding.UTF8);
        File.Move(temporaryPath, currentPath, overwrite: true);
    }

    private long ReadCurrentCheckpointId()
    {
        var currentPath = Path.Combine(_rootPath, CurrentFileName);
        if (!File.Exists(currentPath))
        {
            throw new FileNotFoundException("World checkpoint pointer is missing.", currentPath);
        }

        var text = File.ReadAllText(currentPath, Encoding.UTF8).Trim().TrimStart('\uFEFF');
        if (!long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var checkpointId)
            || checkpointId <= 0)
        {
            throw new InvalidDataException("World checkpoint pointer is invalid.");
        }

        return checkpointId;
    }

    private string CheckpointPath(long checkpointId) =>
        Path.Combine(_rootPath, $"checkpoint-{checkpointId:D20}");

    private static string PartitionPath(string checkpointPath, SimulationScopeId scopeId) =>
        Path.Combine(checkpointPath, "partitions", $"scope-{scopeId.Value:D20}.json");

    private static void ValidateCheckpoint(WorldCheckpointState checkpoint)
    {
        var manifest = checkpoint.Manifest;
        if (manifest.CheckpointId <= 0)
        {
            throw new InvalidOperationException("World checkpoint must have a positive checkpoint ID before persistence.");
        }

        var descriptors = manifest.Partitions.OrderBy(entry => entry.ScopeId.Value).ToArray();
        var partitions = checkpoint.Partitions.OrderBy(entry => entry.ScopeId.Value).ToArray();
        if (descriptors.Length != partitions.Length)
        {
            throw new InvalidOperationException("World checkpoint manifest and partition set have different sizes.");
        }

        for (var index = 0; index < descriptors.Length; index++)
        {
            if (descriptors[index].ScopeId != partitions[index].ScopeId
                || descriptors[index].Revision != partitions[index].Revision
                || !string.Equals(descriptors[index].Kind, WorldPartitionKinds.Settlement, StringComparison.Ordinal)
                || partitions[index].Settlement.ScopeId != partitions[index].ScopeId
                || partitions[index].Settlement.WorldSeed != manifest.WorldSeed
                || partitions[index].Settlement.Time != manifest.Time)
            {
                throw new InvalidOperationException("World checkpoint partition metadata is inconsistent.");
            }
        }
    }
}
