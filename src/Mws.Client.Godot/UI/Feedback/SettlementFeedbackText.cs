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
            SettlementResultCodes.FedResident => $"{resident.Name} ate one ration.",
            SettlementResultCodes.ItemGiven =>
                $"{resident.Name} received {Fact(result, SettlementFactKeys.Quantity, "?")} x {Fact(result, SettlementFactKeys.ItemId, "item")}.",
            SettlementResultCodes.WorkInfo =>
                $"{resident.Name} is a {Fact(result, SettlementFactKeys.Profession, resident.Profession.ToString())} " +
                $"working at {Fact(result, SettlementFactKeys.WorkplaceName, resident.WorkplaceName)}.",
            SettlementResultCodes.Encouraged => $"{resident.Name} seems more confident.",
            SettlementResultCodes.RationShared => $"{resident.Name} appreciates the shared ration.",
            SettlementResultCodes.ResidentNotFound => "Resident is no longer available.",
            SettlementResultCodes.NoRations => "The settlement stockpile has no rations.",
            SettlementResultCodes.InvalidQuantity => "Quantity must be positive.",
            SettlementResultCodes.ItemNotAvailable =>
                $"Stockpile lacks {Fact(result, SettlementFactKeys.Quantity, "?")} x {Fact(result, SettlementFactKeys.ItemId, "item")}.",
            _ when result.Success => "Action completed.",
            _ => $"Action failed: {result.Code}",
        };
    }

    private static string Fact(SettlementCommandResult result, string key, string fallback) =>
        result.Facts.FirstOrDefault(fact => string.Equals(fact.Key, key, StringComparison.Ordinal))?.Value
        ?? fallback;
}
