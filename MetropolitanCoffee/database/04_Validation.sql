-- Read-only structural and business validation. Run after 01, 02 and
-- optionally 05_ImportB04Data.sql. Do not run demo seed 03 for B04 migration.
-- Historical B04 stock is represented by opening movements, not invented
-- purchase/sale movements for old transactions.
USE [MetropolitanCoffeeDB];
GO

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL OR
   OBJECT_ID(N'dbo.Users', N'U') IS NULL OR
   OBJECT_ID(N'dbo.Products', N'U') IS NULL OR
   OBJECT_ID(N'dbo.Orders', N'U') IS NULL OR
   OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL OR
   OBJECT_ID(N'dbo.PurchaseOrders', N'U') IS NULL OR
   OBJECT_ID(N'dbo.PurchaseOrderItems', N'U') IS NULL OR
   OBJECT_ID(N'dbo.CartItems', N'U') IS NULL OR
   OBJECT_ID(N'dbo.StockMovements', N'U') IS NULL
    THROW 50001, 'Missing table. Run 02_CreateTables.sql first.', 1;
GO

SELECT N'Categories' AS TableName, COUNT_BIG(*) AS [RowCount] FROM dbo.Categories
UNION ALL SELECT N'Users', COUNT_BIG(*) FROM dbo.Users
UNION ALL SELECT N'Products', COUNT_BIG(*) FROM dbo.Products
UNION ALL SELECT N'Orders', COUNT_BIG(*) FROM dbo.Orders
UNION ALL SELECT N'OrderItems', COUNT_BIG(*) FROM dbo.OrderItems
UNION ALL SELECT N'PurchaseOrders', COUNT_BIG(*) FROM dbo.PurchaseOrders
UNION ALL SELECT N'PurchaseOrderItems', COUNT_BIG(*) FROM dbo.PurchaseOrderItems
UNION ALL SELECT N'CartItems', COUNT_BIG(*) FROM dbo.CartItems
UNION ALL SELECT N'StockMovements', COUNT_BIG(*) FROM dbo.StockMovements;
GO

SELECT
    (SELECT COUNT(*) FROM sys.foreign_keys WHERE parent_object_id IN (
        OBJECT_ID(N'dbo.Products'), OBJECT_ID(N'dbo.Orders'), OBJECT_ID(N'dbo.OrderItems'),
        OBJECT_ID(N'dbo.PurchaseOrderItems'), OBJECT_ID(N'dbo.CartItems'), OBJECT_ID(N'dbo.StockMovements')
    )) AS ForeignKeyCount,
    (SELECT COUNT(*) FROM sys.foreign_keys WHERE is_disabled = 1 OR is_not_trusted = 1) AS DisabledOrUntrustedForeignKeys,
    (SELECT COUNT(*) FROM sys.check_constraints WHERE is_disabled = 1 OR is_not_trusted = 1) AS DisabledOrUntrustedChecks;
-- ForeignKeyCount should be 11 for this schema.
GO

SELECT N'Orphan product.category' AS CheckName, COUNT_BIG(*) AS IssueCount
FROM dbo.Products p LEFT JOIN dbo.Categories c ON c.Id = p.CategoryId WHERE c.Id IS NULL
UNION ALL SELECT N'Orphan order.user', COUNT_BIG(*)
FROM dbo.Orders o LEFT JOIN dbo.Users u ON u.Id = o.UserId WHERE u.Id IS NULL
UNION ALL SELECT N'Orphan order item.order', COUNT_BIG(*)
FROM dbo.OrderItems oi LEFT JOIN dbo.Orders o ON o.Id = oi.OrderId WHERE o.Id IS NULL
UNION ALL SELECT N'Orphan order item.product', COUNT_BIG(*)
FROM dbo.OrderItems oi LEFT JOIN dbo.Products p ON p.Id = oi.ProductId WHERE p.Id IS NULL
UNION ALL SELECT N'Orphan purchase item.order', COUNT_BIG(*)
FROM dbo.PurchaseOrderItems poi LEFT JOIN dbo.PurchaseOrders po ON po.Id = poi.OrderId WHERE po.Id IS NULL
UNION ALL SELECT N'Orphan purchase item.product', COUNT_BIG(*)
FROM dbo.PurchaseOrderItems poi LEFT JOIN dbo.Products p ON p.Id = poi.ProductId WHERE p.Id IS NULL
UNION ALL SELECT N'Orphan cart.user', COUNT_BIG(*)
FROM dbo.CartItems ci LEFT JOIN dbo.Users u ON u.Id = ci.UserId WHERE u.Id IS NULL
UNION ALL SELECT N'Orphan cart.product', COUNT_BIG(*)
FROM dbo.CartItems ci LEFT JOIN dbo.Products p ON p.Id = ci.ProductId WHERE p.Id IS NULL
UNION ALL SELECT N'Orphan stock.product', COUNT_BIG(*)
FROM dbo.StockMovements sm LEFT JOIN dbo.Products p ON p.Id = sm.ProductId WHERE p.Id IS NULL
UNION ALL SELECT N'Orphan stock.order item or wrong product', COUNT_BIG(*)
FROM dbo.StockMovements sm LEFT JOIN dbo.OrderItems oi ON oi.Id = sm.OrderItemId AND oi.ProductId = sm.ProductId
WHERE sm.OrderItemId IS NOT NULL AND oi.Id IS NULL
UNION ALL SELECT N'Orphan stock.purchase item or wrong product', COUNT_BIG(*)
FROM dbo.StockMovements sm LEFT JOIN dbo.PurchaseOrderItems poi ON poi.Id = sm.PurchaseOrderItemId AND poi.ProductId = sm.ProductId
WHERE sm.PurchaseOrderItemId IS NOT NULL AND poi.Id IS NULL;
GO

SELECT N'Order total differs from item subtotal' AS CheckName, COUNT_BIG(*) AS IssueCount
FROM dbo.Orders o
OUTER APPLY (
    SELECT SUM(CONVERT(DECIMAL(18,2), oi.Price) * oi.Quantity) AS ItemTotal
    FROM dbo.OrderItems oi WHERE oi.OrderId = o.Id
) totals
WHERE o.Total <> ISNULL(totals.ItemTotal, 0)
UNION ALL
SELECT N'Product balance differs from movement ledger', COUNT_BIG(*)
FROM dbo.Products p
LEFT JOIN (
    SELECT ProductId, SUM(CONVERT(BIGINT, QuantityDelta)) AS Balance
    FROM dbo.StockMovements GROUP BY ProductId
) movements ON movements.ProductId = p.Id
WHERE CONVERT(BIGINT, p.Quantity) <> ISNULL(movements.Balance, 0);
GO

SELECT N'Negative product stock' AS CheckName, COUNT_BIG(*) AS IssueCount
FROM dbo.Products WHERE Quantity < 0
UNION ALL SELECT N'Negative cost or profit percent', COUNT_BIG(*)
FROM dbo.Products WHERE CostPrice < 0 OR ProfitPercent < 0
UNION ALL SELECT N'Negative order total', COUNT_BIG(*)
FROM dbo.Orders WHERE Total < 0
UNION ALL SELECT N'Invalid sale item price or quantity', COUNT_BIG(*)
FROM dbo.OrderItems WHERE Price < 0 OR Quantity <= 0
UNION ALL SELECT N'Invalid receipt item price or quantity', COUNT_BIG(*)
FROM dbo.PurchaseOrderItems WHERE ImportPrice < 0 OR Quantity <= 0;
GO
