namespace MetropolitanCoffee.Core.Models;

public sealed record AdminDashboardOrders(int Count, int LatestId, DateTime? LatestUpdatedAt);

public sealed record AdminDashboardSnapshot(int Users, int Products, AdminDashboardOrders Orders);
