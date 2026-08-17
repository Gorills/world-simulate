using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class SettlementHouseholdTests
{
    [Fact]
    public void DefaultVillageProjectsPersistedHomesAndHouseholds()
    {
        var simulation = SettlementSimulation.CreateDefault(new WorldSeed(601));
        var projection = simulation.Project();
        var homes = projection.Homes
            ?? throw new InvalidOperationException("Home projection is missing.");
        var households = projection.Households
            ?? throw new InvalidOperationException("Household projection is missing.");
        var mira = projection.Residents.Single(resident => resident.Name == "Mira");
        var tor = projection.Residents.Single(resident => resident.Name == "Tor");
        var ena = projection.Residents.Single(resident => resident.Name == "Ena");

        Assert.Equal(10, homes.Count);
        Assert.Equal(2, households.Count);
        Assert.Equal(mira.HouseholdId, tor.HouseholdId);
        Assert.Equal(mira.HomeId, tor.HomeId);
        Assert.NotEqual(default(EntityId), mira.HomeId);
        Assert.NotEqual(mira.HouseholdId, ena.HouseholdId);
        Assert.NotEqual(mira.HomeId, ena.HomeId);
        Assert.Equal(2, homes.Single(home => home.Id == mira.HomeId).ResidentCount);
        Assert.Equal(1, homes.Single(home => home.Id == ena.HomeId).ResidentCount);
        Assert.Equal(8, homes.Count(home => home.ResidentCount == 0));

        var json = SettlementStateJson.Serialize(simulation.CaptureState());
        var restored = SettlementSimulation.Restore(SettlementStateJson.Deserialize(json));

        Assert.Equal(json, SettlementStateJson.Serialize(restored.CaptureState()));
    }

    [Fact]
    public void ResidenceValidationRejectsHouseholdOverHomeCapacity()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(602)).CaptureState();
        var mira = state.Residents.Single(resident => resident.Name == "Mira");
        var household = (state.Households ?? [])
            .Single(entry => entry.Id == mira.HouseholdId);
        var homes = (state.Homes ?? [])
            .Select(home => home.Id == household.HomeId ? home with { Capacity = 1 } : home)
            .ToArray();

        Assert.Throws<InvalidOperationException>(() =>
            SettlementSimulation.Restore(state with { Homes = homes }));
    }

    [Fact]
    public void PreviousV4SnapshotMigratesWithoutInventingResidenceAssignments()
    {
        var current = SettlementSimulation.CreateDefault(new WorldSeed(603)).CaptureState();
        var legacyResidents = current.Residents.Select(resident => new
        {
            resident.Id,
            resident.Name,
            resident.Hunger,
            resident.Energy,
            resident.Activity,
            resident.Profession,
            resident.WorkplaceId,
            resident.Affinity,
        }).ToArray();
        var legacyPayload = JsonSerializer.Serialize(new
        {
            SchemaVersion = SettlementVersions.PreviousSchemaVersion,
            current.ModelVersion,
            current.RulesVersion,
            current.ContentVersion,
            current.ScopeId,
            current.WorldSeed,
            current.Time,
            current.NextEventId,
            current.NextStackId,
            current.NextCommandId,
            current.SettlementOwnerId,
            Residents = legacyResidents,
            current.ItemStacks,
            current.Workplaces,
            current.Events,
            current.CommandReceipts,
        });
        var checksum = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(legacyPayload)));
        var envelope = JsonSerializer.Serialize(new
        {
            SchemaVersion = SettlementVersions.PreviousSchemaVersion,
            Payload = legacyPayload,
            Checksum = checksum,
        });

        var migrated = SettlementStateJson.Deserialize(envelope);
        var restored = SettlementSimulation.Restore(migrated);

        Assert.Equal(SettlementVersions.CurrentSchemaVersion, migrated.SchemaVersion);
        Assert.Empty(migrated.Homes ?? []);
        Assert.Empty(migrated.Households ?? []);
        Assert.All(migrated.Residents, resident => Assert.Equal(default(EntityId), resident.HouseholdId));
        Assert.All(restored.Project().Residents, resident => Assert.Equal(default(EntityId), resident.HomeId));
    }
}
