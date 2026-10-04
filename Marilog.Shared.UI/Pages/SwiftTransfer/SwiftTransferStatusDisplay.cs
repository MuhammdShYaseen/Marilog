using Marilog.Contracts.DTOs.Responses;
using Marilog.Kernel.Enums;
using MudBlazor;

namespace Marilog.Shared.UI.Pages.SwiftTransfer;

public static class SwiftTransferStatusDisplay
{
    public readonly record struct Info(string Text, string Icon, Color Color, string Tooltip);

    public static Info For(SwiftTransferResponse t) => t.Status switch
    {
        SwiftTransferStatus.Cancelled =>
            new("Cancelled", Icons.Material.Filled.Block, Color.Error,
                t.CancellationReason ?? "Cancelled"),

        SwiftTransferStatus.Pending =>
            new($"Pending · {DaysPending(t)}d", Icons.Material.Filled.Schedule, Color.Info,
                "Awaiting bank confirmation"),

        _ when t.AllocatedAmount <= 0 =>
            new("Unallocated", Icons.Material.Outlined.Circle, Color.Warning,
                "Received, nothing allocated yet"),

        _ when !t.IsFullyAllocated =>
            new("Partial", Icons.Material.Filled.Timelapse, Color.Warning,
                $"{t.UnallocatedAmount:N2} {t.CurrencyCode} left to allocate"),

        _ => new("Allocated", Icons.Material.Filled.CheckCircle, Color.Success,
                 "Fully allocated")
    };

    public static int DaysPending(SwiftTransferResponse t) =>
        DateOnly.FromDateTime(DateTime.Today).DayNumber - t.TransactionDate.DayNumber;
}