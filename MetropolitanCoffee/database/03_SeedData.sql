-- OPTIONAL DEMO ONLY. DO NOT RUN for B04 migration.
-- 05_ImportB04Data.sql requires all business tables to be empty.
﻿-- Optional, idempotent demonstration data. Negative IDs are reserved for samples;
-- original B04 AUTO_INCREMENT IDs remain available for a later migration.
-- No users, passwords, orders or inventory balances are seeded.
USE [MetropolitanCoffeeDB];
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = -1001 OR Name = N'Demo - Cà phê')
   OR NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = -1002 OR Name = N'Demo - Trà')
BEGIN
    SET IDENTITY_INSERT dbo.Categories ON;
    INSERT INTO dbo.Categories (Id, Name)
    SELECT -1001, N'Demo - Cà phê'
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = -1001 OR Name = N'Demo - Cà phê');
    INSERT INTO dbo.Categories (Id, Name)
    SELECT -1002, N'Demo - Trà'
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Categories WHERE Id = -1002 OR Name = N'Demo - Trà');
    SET IDENTITY_INSERT dbo.Categories OFF;
END;
GO

SET IDENTITY_INSERT dbo.Products ON;
INSERT INTO dbo.Products
    (Id, CategoryId, Name, ShortDescription, CostPrice, ProfitPercent, Quantity, Status, HasSize, HasIce, HasSugar)
SELECT -1001, c.Id, N'Demo - Cà phê sữa', N'Sản phẩm mẫu, chưa có tồn kho', 0, 25, 0, N'inactive', 1, 1, 1
FROM dbo.Categories AS c
WHERE c.Name = N'Demo - Cà phê'
  AND NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = -1001 OR Name = N'Demo - Cà phê sữa');

INSERT INTO dbo.Products
    (Id, CategoryId, Name, ShortDescription, CostPrice, ProfitPercent, Quantity, Status, HasSize, HasIce, HasSugar)
SELECT -1002, c.Id, N'Demo - Trà đào', N'Sản phẩm mẫu, chưa có tồn kho', 0, 25, 0, N'inactive', 1, 1, 1
FROM dbo.Categories AS c
WHERE c.Name = N'Demo - Trà'
  AND NOT EXISTS (SELECT 1 FROM dbo.Products WHERE Id = -1002 OR Name = N'Demo - Trà đào');
SET IDENTITY_INSERT dbo.Products OFF;
GO
