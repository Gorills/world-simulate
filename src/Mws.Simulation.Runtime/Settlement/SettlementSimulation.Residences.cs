using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private readonly List<HomeState> _homes;
    private readonly List<HouseholdState> _households;
    private readonly Dictionary<EntityId, HomeState> _homesById = new();
    private readonly Dictionary<EntityId, HouseholdState> _householdsById = new();

    private void RebuildResidenceIndexes()
    {
        _homesById.Clear();
        foreach (var home in _homes)
        {
            _homesById.Add(home.Id, home);
        }

        _householdsById.Clear();
        foreach (var household in _households)
        {
            _householdsById.Add(household.Id, household);
        }
    }

    private void ValidateResidenceState()
    {
        EnsureUnique(_homes.Select(home => home.Id.Value), "home");
        EnsureUnique(_households.Select(household => household.Id.Value), "household");

        if (_homes.Any(home =>
            home.Id.Value <= 0
            || string.IsNullOrWhiteSpace(home.Name)
            || string.IsNullOrWhiteSpace(home.SpatialKey)
            || home.Capacity <= 0))
        {
            throw new InvalidOperationException("Settlement state contains an invalid home.");
        }

        if (_households.Any(household =>
            household.Id.Value <= 0
            || household.HomeId.Value <= 0
            || string.IsNullOrWhiteSpace(household.Name)))
        {
            throw new InvalidOperationException("Settlement state contains an invalid household.");
        }

        var homeIds = _homes.Select(home => home.Id).ToHashSet();
        if (_households.Any(household => !homeIds.Contains(household.HomeId)))
        {
            throw new InvalidOperationException("Settlement household references a missing home.");
        }

        if (_households.GroupBy(household => household.HomeId).Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException("A settlement home cannot be assigned to multiple households.");
        }

        foreach (var resident in _residents)
        {
            if (resident.HouseholdId.Value != 0 && !_households.Any(entry => entry.Id == resident.HouseholdId))
            {
                throw new InvalidOperationException(
                    $"Resident {resident.Id.Value} references a missing household.");
            }
        }

        foreach (var household in _households)
        {
            var home = _homes.Single(entry => entry.Id == household.HomeId);
            var residentCount = _residents.Count(resident => resident.HouseholdId == household.Id);
            if (residentCount > home.Capacity)
            {
                throw new InvalidOperationException(
                    $"Household {household.Id.Value} exceeds home {home.Id.Value} capacity.");
            }
        }
    }

    private HomeState? FindHome(EntityId homeId) =>
        _homesById.TryGetValue(homeId, out var home) ? home : null;

    private HouseholdState? FindHousehold(EntityId householdId) =>
        _householdsById.TryGetValue(householdId, out var household) ? household : null;

    private HomeProjection[] ProjectHomes()
    {
        var residentCounts = new Dictionary<EntityId, int>();
        foreach (var resident in _residents)
        {
            var household = FindHousehold(resident.HouseholdId);
            if (household is null)
            {
                continue;
            }

            residentCounts.TryGetValue(household.HomeId, out var current);
            residentCounts[household.HomeId] = checked(current + 1);
        }

        return _homes
            .OrderBy(home => home.Id.Value)
            .Select(home => new HomeProjection(
                home.Id,
                home.Name,
                home.SpatialKey,
                home.Capacity,
                residentCounts.GetValueOrDefault(home.Id)))
            .ToArray();
    }

    private HouseholdProjection[] ProjectHouseholds() =>
        _households
            .OrderBy(household => household.Id.Value)
            .Select(household =>
            {
                var home = FindHome(household.HomeId)
                    ?? throw new InvalidOperationException("Validated household home is missing.");
                var residentIds = _residents
                    .Where(resident => resident.HouseholdId == household.Id)
                    .Select(resident => resident.Id)
                    .OrderBy(id => id.Value)
                    .ToArray();
                return new HouseholdProjection(
                    household.Id,
                    household.Name,
                    home.Id,
                    home.Name,
                    residentIds);
            })
            .ToArray();
}
