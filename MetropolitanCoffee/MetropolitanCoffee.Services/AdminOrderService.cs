
using MetropolitanCoffee.Core.Repositories;

namespace MetropolitanCoffee.Services;

public sealed record AdminOrderResult(
    bool Success,
    string Message
);

public sealed class AdminOrderService(
    IAdminOrderRepository repository)
{
    public async Task<AdminOrderResult> UpdateStatusAsync(
        int orderId,
        string currentStatus,
        CancellationToken ct = default)
    {
        if (orderId <= 0)
            return new(false, "Please select an order.");


        string? nextStatus = currentStatus.Trim().ToLowerInvariant() switch
        {
            "pending" => "confirmed",
            "confirmed" => "shipping",
            "shipping" => "delivered",
            _ => null
        };


        if (nextStatus is null)
            return new(false,
                "This order cannot be updated.");

        bool updated = await repository.UpdateStatusAsync(
            orderId, currentStatus, nextStatus, ct);

        return updated
            ? new(true, $"Order updated to {nextStatus}.")
            : new(false,
                "Order status has changed or order was not found.");
    }


    public async Task<AdminOrderResult> ChangeStatusAsync(
        int orderId,
        string currentStatus,
        string newStatus,
        CancellationToken ct = default)
    {
        if (orderId <= 0)
            return new(false, "Please select an order.");

        currentStatus = currentStatus?.Trim().ToLowerInvariant() ?? "";
        newStatus = newStatus?.Trim().ToLowerInvariant() ?? "";

        string[] statuses =
        {
            "pending",
            "confirmed",
            "shipping",
            "delivered"
        };

        if (newStatus == "cancelled")
        {
            if (currentStatus is not ("pending" or "confirmed"))
                return new(false, "This order cannot be cancelled.");

            return await CancelOrderAsync(orderId, ct);
        }

        int currentIndex = Array.IndexOf(statuses, currentStatus);
        int newIndex = Array.IndexOf(statuses, newStatus);

        if (currentIndex < 0 || newIndex <= currentIndex)
            return new(false, "Invalid order status transition.");

        bool updated = await repository.UpdateStatusAsync(
            orderId,
            currentStatus,
            newStatus,
            ct);

        return updated
            ? new(true, $"Order updated to {newStatus}.")
            : new(false, "Order status has changed or order was not found.");
    }

    public async Task<AdminOrderResult> CancelOrderAsync(
        int orderId,
        CancellationToken ct = default)
    {
        if (orderId <= 0)
            return new(false, "Please select an order.");

        bool cancelled = await repository.CancelOrderAsync(
            orderId, ct);

        return cancelled
            ? new(true, "Order cancelled successfully.")
            : new(false,
                "Only pending orders can be cancelled.");
    }
}
