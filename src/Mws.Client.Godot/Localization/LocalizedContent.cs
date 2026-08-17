using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.Localization;

internal static class LocalizedContent
{
    internal static string Profession(ResidentProfession profession) => profession switch
    {
        ResidentProfession.Farmer => GameLocalization.Tr("CONTENT_PROFESSION_FARMER"),
        ResidentProfession.Cook => GameLocalization.Tr("CONTENT_PROFESSION_COOK"),
        ResidentProfession.Forager => GameLocalization.Tr("CONTENT_PROFESSION_FORAGER"),
        _ => profession.ToString(),
    };

    internal static string Profession(string value, ResidentProfession fallback) =>
        Enum.TryParse<ResidentProfession>(value, ignoreCase: true, out var profession)
            ? Profession(profession)
            : Profession(fallback);

    internal static string Activity(ResidentActivity activity) => activity switch
    {
        ResidentActivity.Idle => GameLocalization.Tr("CONTENT_ACTIVITY_IDLE"),
        ResidentActivity.Working => GameLocalization.Tr("CONTENT_ACTIVITY_WORKING"),
        ResidentActivity.Eating => GameLocalization.Tr("CONTENT_ACTIVITY_EATING"),
        ResidentActivity.Resting => GameLocalization.Tr("CONTENT_ACTIVITY_RESTING"),
        _ => activity.ToString(),
    };

    internal static string Item(string itemId) => itemId switch
    {
        SettlementItems.Grain => GameLocalization.Tr("CONTENT_ITEM_GRAIN"),
        SettlementItems.Ration => GameLocalization.Tr("CONTENT_ITEM_RATION"),
        SettlementItems.Herb => GameLocalization.Tr("CONTENT_ITEM_HERB"),
        _ => itemId,
    };

    internal static string Workplace(string name) => name switch
    {
        "North Field" => GameLocalization.Tr("CONTENT_WORKPLACE_NORTH_FIELD"),
        "Common Kitchen" => GameLocalization.Tr("CONTENT_WORKPLACE_COMMON_KITCHEN"),
        "Herb Grove" => GameLocalization.Tr("CONTENT_WORKPLACE_HERB_GROVE"),
        _ => name,
    };

    internal static string Household(string name) => name switch
    {
        "North Household" => GameLocalization.Tr("CONTENT_HOUSEHOLD_NORTH"),
        "East Household" => GameLocalization.Tr("CONTENT_HOUSEHOLD_EAST"),
        "Miller Household" => GameLocalization.Tr("CONTENT_HOUSEHOLD_MILLER"),
        "Cook Household" => GameLocalization.Tr("CONTENT_HOUSEHOLD_COOK"),
        "River Household" => GameLocalization.Tr("CONTENT_HOUSEHOLD_RIVER"),
        "Grove Household" => GameLocalization.Tr("CONTENT_HOUSEHOLD_GROVE"),
        _ => name,
    };

    internal static string Home(SettlementProjection projection, EntityId homeId)
    {
        ArgumentNullException.ThrowIfNull(projection);
        var home = projection.Homes?.FirstOrDefault(entry => entry.Id == homeId);
        return home is null
            ? GameLocalization.Tr("CONTENT_HOME_UNKNOWN")
            : HomeBySpatialKey(home.SpatialKey, home.Name);
    }

    internal static string Building(string name) => name switch
    {
        "North House West" => GameLocalization.Tr("CONTENT_BUILDING_NORTH_HOUSE_WEST"),
        "North House East" => GameLocalization.Tr("CONTENT_BUILDING_NORTH_HOUSE_EAST"),
        "Miller House" => GameLocalization.Tr("CONTENT_BUILDING_MILLER_HOUSE"),
        "Cook House" => GameLocalization.Tr("CONTENT_BUILDING_COOK_HOUSE"),
        "River House" => GameLocalization.Tr("CONTENT_BUILDING_RIVER_HOUSE"),
        "Grove House" => GameLocalization.Tr("CONTENT_BUILDING_GROVE_HOUSE"),
        "South House West" => GameLocalization.Tr("CONTENT_BUILDING_SOUTH_HOUSE_WEST"),
        "South House East" => GameLocalization.Tr("CONTENT_BUILDING_SOUTH_HOUSE_EAST"),
        "Far South House West" => GameLocalization.Tr("CONTENT_BUILDING_FAR_SOUTH_HOUSE_WEST"),
        "Far South House East" => GameLocalization.Tr("CONTENT_BUILDING_FAR_SOUTH_HOUSE_EAST"),
        "The Hearth Inn" => GameLocalization.Tr("CONTENT_BUILDING_HEARTH_INN"),
        "Carpenter Workshop" => GameLocalization.Tr("CONTENT_BUILDING_CARPENTER_WORKSHOP"),
        "Common Storehouse" => GameLocalization.Tr("CONTENT_BUILDING_COMMON_STOREHOUSE"),
        "South Barn" => GameLocalization.Tr("CONTENT_BUILDING_SOUTH_BARN"),
        _ => name,
    };

    private static string HomeBySpatialKey(string spatialKey, string fallback) => spatialKey switch
    {
        SettlementHomeSpatialKeys.NorthWest => GameLocalization.Tr("CONTENT_BUILDING_NORTH_HOUSE_WEST"),
        SettlementHomeSpatialKeys.NorthEast => GameLocalization.Tr("CONTENT_BUILDING_NORTH_HOUSE_EAST"),
        SettlementHomeSpatialKeys.Miller => GameLocalization.Tr("CONTENT_BUILDING_MILLER_HOUSE"),
        SettlementHomeSpatialKeys.Cook => GameLocalization.Tr("CONTENT_BUILDING_COOK_HOUSE"),
        SettlementHomeSpatialKeys.River => GameLocalization.Tr("CONTENT_BUILDING_RIVER_HOUSE"),
        SettlementHomeSpatialKeys.Grove => GameLocalization.Tr("CONTENT_BUILDING_GROVE_HOUSE"),
        SettlementHomeSpatialKeys.SouthWest => GameLocalization.Tr("CONTENT_BUILDING_SOUTH_HOUSE_WEST"),
        SettlementHomeSpatialKeys.SouthEast => GameLocalization.Tr("CONTENT_BUILDING_SOUTH_HOUSE_EAST"),
        SettlementHomeSpatialKeys.FarSouthWest => GameLocalization.Tr("CONTENT_BUILDING_FAR_SOUTH_HOUSE_WEST"),
        SettlementHomeSpatialKeys.FarSouthEast => GameLocalization.Tr("CONTENT_BUILDING_FAR_SOUTH_HOUSE_EAST"),
        _ => fallback,
    };
}
