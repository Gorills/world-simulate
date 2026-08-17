using Mws.Client.Godot.Localization;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.UI.Feedback;

internal static class SettlementFeedbackText
{
    internal static string Format(SettlementCommandResult result, ResidentProjection resident)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(resident);

        return result.Code switch
        {
            SettlementResultCodes.FedResident =>
                GameLocalization.Format("FEEDBACK_FED_RESIDENT", resident.Name),
            SettlementResultCodes.ItemGiven =>
                GameLocalization.Format(
                    "FEEDBACK_ITEM_GIVEN",
                    resident.Name,
                    Fact(result, SettlementFactKeys.Quantity, "?"),
                    LocalizedContent.Item(Fact(result, SettlementFactKeys.ItemId, string.Empty))),
            SettlementResultCodes.WorkInfo =>
                GameLocalization.Format(
                    "FEEDBACK_WORK_INFO",
                    resident.Name,
                    LocalizedContent.Profession(
                        Fact(result, SettlementFactKeys.Profession, resident.Profession.ToString()),
                        resident.Profession),
                    LocalizedContent.Workplace(
                        Fact(result, SettlementFactKeys.WorkplaceName, resident.WorkplaceName))),
            SettlementResultCodes.Encouraged =>
                GameLocalization.Format("FEEDBACK_ENCOURAGED", resident.Name),
            SettlementResultCodes.RationShared =>
                GameLocalization.Format("FEEDBACK_RATION_SHARED", resident.Name),
            SettlementResultCodes.ResidentNotFound => GameLocalization.Tr("FEEDBACK_RESIDENT_NOT_FOUND"),
            SettlementResultCodes.NoRations => GameLocalization.Tr("FEEDBACK_NO_RATIONS"),
            SettlementResultCodes.InvalidQuantity => GameLocalization.Tr("FEEDBACK_INVALID_QUANTITY"),
            SettlementResultCodes.ItemNotAvailable =>
                GameLocalization.Format(
                    "FEEDBACK_ITEM_NOT_AVAILABLE",
                    Fact(result, SettlementFactKeys.Quantity, "?"),
                    LocalizedContent.Item(Fact(result, SettlementFactKeys.ItemId, string.Empty))),
            SettlementResultCodes.InventoryCapacityExceeded =>
                GameLocalization.Tr("FEEDBACK_INVENTORY_CAPACITY_EXCEEDED"),
            SettlementResultCodes.StaleCommand => GameLocalization.Tr("FEEDBACK_STALE_COMMAND"),
            _ when result.Success => GameLocalization.Tr("FEEDBACK_ACTION_COMPLETED"),
            _ => GameLocalization.Format("FEEDBACK_ACTION_FAILED", result.Code),
        };
    }

    private static string Fact(SettlementCommandResult result, string key, string fallback) =>
        result.Facts.FirstOrDefault(fact => string.Equals(fact.Key, key, StringComparison.Ordinal))?.Value
        ?? fallback;
}
