using Mws.Domain;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class SettlementProjectionAssignmentTests
{
    [Fact]
    public void ResidentProjectionCarriesAuthoritativeWorkplaceId()
    {
        var projection = SettlementSimulation.CreateDefault(new WorldSeed(607)).Project();
        var workplaceIds = projection.Workplaces.Select(workplace => workplace.Id).ToHashSet();

        Assert.All(projection.Residents, resident =>
        {
            Assert.NotEqual(default(EntityId), resident.WorkplaceId);
            Assert.Contains(resident.WorkplaceId, workplaceIds);
        });
    }
}
