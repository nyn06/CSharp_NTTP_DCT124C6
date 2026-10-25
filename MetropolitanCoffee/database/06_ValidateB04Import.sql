-- Read-only validation for the exact B04 dump imported by 05.
-- Run immediately after 05, before the Web/Admin writes new business rows.
-- 04_Validation.sql remains the generic check for later use.
USE [MetropolitanCoffeeDB];
GO
DECLARE @Checks TABLE (CheckName NVARCHAR(100), Expected DECIMAL(20,2), Actual DECIMAL(20,2));
INSERT INTO @Checks SELECT N'Categories rows', 4, COUNT_BIG(*) FROM dbo.[Categories];
INSERT INTO @Checks SELECT N'Users rows', 6, COUNT_BIG(*) FROM dbo.[Users];
INSERT INTO @Checks SELECT N'Products rows', 37, COUNT_BIG(*) FROM dbo.[Products];
INSERT INTO @Checks SELECT N'Orders rows', 9, COUNT_BIG(*) FROM dbo.[Orders];
INSERT INTO @Checks SELECT N'OrderItems rows', 19, COUNT_BIG(*) FROM dbo.[OrderItems];
INSERT INTO @Checks SELECT N'PurchaseOrders rows', 14, COUNT_BIG(*) FROM dbo.[PurchaseOrders];
INSERT INTO @Checks SELECT N'PurchaseOrderItems rows', 48, COUNT_BIG(*) FROM dbo.[PurchaseOrderItems];
INSERT INTO @Checks SELECT N'CartItems rows', 0, COUNT_BIG(*) FROM dbo.CartItems;
INSERT INTO @Checks SELECT N'Opening movements', 37, COUNT_BIG(*) FROM dbo.StockMovements WHERE MovementType=N'opening';
INSERT INTO @Checks SELECT N'Product stock units', 3582, ISNULL(SUM(CONVERT(BIGINT,Quantity)),0) FROM dbo.Products;
INSERT INTO @Checks SELECT N'Opening movement units', 3582, ISNULL(SUM(CONVERT(BIGINT,QuantityDelta)),0) FROM dbo.StockMovements;
INSERT INTO @Checks SELECT N'Order totals', 105.32, ISNULL(SUM(CONVERT(DECIMAL(18,2),Total)),0) FROM dbo.Orders;
INSERT INTO @Checks SELECT N'Order item extended totals', 105.32, ISNULL(SUM(CONVERT(DECIMAL(18,2),Price)*Quantity),0) FROM dbo.OrderItems;
INSERT INTO @Checks SELECT N'Purchase item extended totals', 7555.88, ISNULL(SUM(CONVERT(DECIMAL(18,2),ImportPrice)*Quantity),0) FROM dbo.PurchaseOrderItems;
INSERT INTO @Checks SELECT N'Product cost-price sum', 73.77, ISNULL(SUM(CONVERT(DECIMAL(18,2),CostPrice)),0) FROM dbo.Products;
INSERT INTO @Checks SELECT N'Categories identity current', 18, IDENT_CURRENT(N'dbo.Categories');
INSERT INTO @Checks SELECT N'Users identity current', 28, IDENT_CURRENT(N'dbo.Users');
INSERT INTO @Checks SELECT N'Products identity current', 74, IDENT_CURRENT(N'dbo.Products');
INSERT INTO @Checks SELECT N'Orders identity current', 123, IDENT_CURRENT(N'dbo.Orders');
INSERT INTO @Checks SELECT N'OrderItems identity current', 150, IDENT_CURRENT(N'dbo.OrderItems');
INSERT INTO @Checks SELECT N'PurchaseOrders identity current', 43, IDENT_CURRENT(N'dbo.PurchaseOrders');
INSERT INTO @Checks SELECT N'PurchaseOrderItems identity current', 80, IDENT_CURRENT(N'dbo.PurchaseOrderItems');
IF EXISTS (SELECT Id FROM (VALUES (1),(2),(3),(4)) AS src(Id) EXCEPT SELECT Id FROM dbo.[Categories])
   OR EXISTS (SELECT Id FROM dbo.[Categories] EXCEPT SELECT Id FROM (VALUES (1),(2),(3),(4)) AS src(Id))
    THROW 51100, 'Categories ID set differs from the B04 dump', 1;
IF EXISTS (SELECT Id FROM (VALUES (5),(6),(25),(26),(27),(28)) AS src(Id) EXCEPT SELECT Id FROM dbo.[Users])
   OR EXISTS (SELECT Id FROM dbo.[Users] EXCEPT SELECT Id FROM (VALUES (5),(6),(25),(26),(27),(28)) AS src(Id))
    THROW 51100, 'Users ID set differs from the B04 dump', 1;
IF EXISTS (SELECT Id FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15),(16),(17),(18),(19),(20),(21),(22),(23),(24),(25),(26),(27),(28),(29),(30),(31),(32),(33),(34),(35),(36),(37)) AS src(Id) EXCEPT SELECT Id FROM dbo.[Products])
   OR EXISTS (SELECT Id FROM dbo.[Products] EXCEPT SELECT Id FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10),(11),(12),(13),(14),(15),(16),(17),(18),(19),(20),(21),(22),(23),(24),(25),(26),(27),(28),(29),(30),(31),(32),(33),(34),(35),(36),(37)) AS src(Id))
    THROW 51100, 'Products ID set differs from the B04 dump', 1;
IF EXISTS (SELECT Id FROM (VALUES (115),(116),(117),(118),(119),(120),(121),(122),(123)) AS src(Id) EXCEPT SELECT Id FROM dbo.[Orders])
   OR EXISTS (SELECT Id FROM dbo.[Orders] EXCEPT SELECT Id FROM (VALUES (115),(116),(117),(118),(119),(120),(121),(122),(123)) AS src(Id))
    THROW 51100, 'Orders ID set differs from the B04 dump', 1;
IF EXISTS (SELECT Id FROM (VALUES (132),(133),(134),(135),(136),(137),(138),(139),(140),(141),(142),(143),(144),(145),(146),(147),(148),(149),(150)) AS src(Id) EXCEPT SELECT Id FROM dbo.[OrderItems])
   OR EXISTS (SELECT Id FROM dbo.[OrderItems] EXCEPT SELECT Id FROM (VALUES (132),(133),(134),(135),(136),(137),(138),(139),(140),(141),(142),(143),(144),(145),(146),(147),(148),(149),(150)) AS src(Id))
    THROW 51100, 'OrderItems ID set differs from the B04 dump', 1;
IF EXISTS (SELECT Id FROM (VALUES (30),(31),(32),(33),(34),(35),(36),(37),(38),(39),(40),(41),(42),(43)) AS src(Id) EXCEPT SELECT Id FROM dbo.[PurchaseOrders])
   OR EXISTS (SELECT Id FROM dbo.[PurchaseOrders] EXCEPT SELECT Id FROM (VALUES (30),(31),(32),(33),(34),(35),(36),(37),(38),(39),(40),(41),(42),(43)) AS src(Id))
    THROW 51100, 'PurchaseOrders ID set differs from the B04 dump', 1;
IF EXISTS (SELECT Id FROM (VALUES (31),(32),(33),(34),(35),(38),(39),(40),(41),(42),(43),(44),(45),(46),(47),(48),(49),(50),(51),(52),(53),(54),(55),(56),(57),(58),(59),(60),(61),(62),(63),(64),(65),(66),(67),(68),(69),(70),(71),(72),(73),(74),(75),(76),(77),(78),(79),(80)) AS src(Id) EXCEPT SELECT Id FROM dbo.[PurchaseOrderItems])
   OR EXISTS (SELECT Id FROM dbo.[PurchaseOrderItems] EXCEPT SELECT Id FROM (VALUES (31),(32),(33),(34),(35),(38),(39),(40),(41),(42),(43),(44),(45),(46),(47),(48),(49),(50),(51),(52),(53),(54),(55),(56),(57),(58),(59),(60),(61),(62),(63),(64),(65),(66),(67),(68),(69),(70),(71),(72),(73),(74),(75),(76),(77),(78),(79),(80)) AS src(Id))
    THROW 51100, 'PurchaseOrderItems ID set differs from the B04 dump', 1;
IF EXISTS (SELECT 1 FROM dbo.Users WHERE PasswordHash NOT LIKE N'$2y$%' OR LEN(PasswordHash) <> 60)
    THROW 51101, 'A B04 bcrypt hash is missing or changed format', 1;
IF EXISTS (SELECT 1 FROM dbo.StockMovements WHERE MovementType <> N'opening' OR OrderItemId IS NOT NULL OR PurchaseOrderItemId IS NOT NULL)
    THROW 51102, 'Historical stock movements were invented during import', 1;
SELECT CheckName, Expected, Actual, CASE WHEN Expected=Actual THEN N'PASS' ELSE N'FAIL' END AS Result FROM @Checks ORDER BY CheckName;
IF EXISTS (SELECT 1 FROM @Checks WHERE Expected <> Actual)
    THROW 51103, 'B04 import aggregate validation failed', 1;
GO
