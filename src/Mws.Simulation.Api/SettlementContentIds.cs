namespace Mws.Simulation.Api;

public static class SettlementVersions
{
    public const int CurrentSchemaVersion = 5;
    public const int PreviousSchemaVersion = 4;
    public const int LegacySchemaVersion = 3;
    public const int LegacyResidentLocationEncodingVersion = 0;
    public const int CurrentResidentLocationEncodingVersion = 1;
    public const string CurrentModelVersion = "settlement-model-v1";
    public const string CurrentRulesVersion = "settlement-rules-v1";
    public const string CurrentContentVersion = "settlement-content-v1";
}

public static class SettlementItems
{
    public const string Ration = "ration";
    public const string Grain = "grain";
    public const string Herb = "herb";
}

public static class SettlementHomeSpatialKeys
{
    public const string NorthWest = "home.north-west";
    public const string NorthEast = "home.north-east";
    public const string Miller = "home.miller";
    public const string Cook = "home.cook";
    public const string River = "home.river";
    public const string Grove = "home.grove";
    public const string SouthWest = "home.south-west";
    public const string SouthEast = "home.south-east";
    public const string FarSouthWest = "home.far-south-west";
    public const string FarSouthEast = "home.far-south-east";
}

public static class SettlementResultCodes
{
    public const string FedResident = "FED_RESIDENT";
    public const string ItemGiven = "ITEM_GIVEN";
    public const string WorkInfo = "WORK_INFO";
    public const string Encouraged = "ENCOURAGED";
    public const string RationShared = "RATION_SHARED";
    public const string ResidentNotFound = "RESIDENT_NOT_FOUND";
    public const string NoRations = "NO_RATIONS";
    public const string InvalidQuantity = "INVALID_QUANTITY";
    public const string ItemNotAvailable = "ITEM_NOT_AVAILABLE";
    public const string InventoryCapacityExceeded = "INVENTORY_CAPACITY_EXCEEDED";
    public const string StaleCommand = "STALE_COMMAND";
    public const string PlayerRequired = "PLAYER_REQUIRED";
    public const string PlayerScopeMismatch = "PLAYER_SCOPE_MISMATCH";
    public const string InteractionActorTravelling = "INTERACTION_ACTOR_TRAVELLING";
    public const string InteractionNotCoLocated = "INTERACTION_NOT_COLOCATED";
}

public static class SettlementEventKinds
{
    public const string PlayerFed = "player-fed";
    public const string ItemGiven = "item-given";
    public const string AskedAboutWork = "asked-about-work";
    public const string Encouraged = "encouraged";
    public const string SharedRation = "shared-ration";
    public const string DayBegan = "day-began";
}

public static class SettlementFactKeys
{
    public const string ResidentName = "resident_name";
    public const string ItemId = "item_id";
    public const string Quantity = "quantity";
    public const string Profession = "profession";
    public const string WorkplaceName = "workplace_name";
    public const string AffinityDelta = "affinity_delta";
    public const string Day = "day";
    public const string Rations = "rations";
}
