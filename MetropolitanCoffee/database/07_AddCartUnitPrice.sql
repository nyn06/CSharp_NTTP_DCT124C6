-- Non-destructive upgrade: B04 stores the database sell price when an item is added.
USE [MetropolitanCoffeeDB];
GO
IF OBJECT_ID(N'dbo.CartItems', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.CartItems', N'UnitPrice') IS NULL
BEGIN
    BEGIN TRANSACTION;
    ALTER TABLE dbo.CartItems ADD UnitPrice DECIMAL(10,2) NULL;
    EXEC(N'UPDATE c SET UnitPrice=p.SellPrice FROM dbo.CartItems AS c INNER JOIN dbo.Products AS p ON p.Id=c.ProductId WHERE c.UnitPrice IS NULL;');
    EXEC(N'ALTER TABLE dbo.CartItems ALTER COLUMN UnitPrice DECIMAL(10,2) NOT NULL;');
    EXEC(N'ALTER TABLE dbo.CartItems ADD CONSTRAINT CK_CartItems_UnitPrice CHECK (UnitPrice >= 0);');
    COMMIT TRANSACTION;
END;
GO
