-- Idempotent, non-destructive migration for checkout request replay protection.
USE [MetropolitanCoffeeDB];
GO
SET ANSI_NULLS ON;
SET ANSI_PADDING ON;
SET ANSI_WARNINGS ON;
SET ARITHABORT ON;
SET CONCAT_NULL_YIELDS_NULL ON;
SET QUOTED_IDENTIFIER ON;
SET NUMERIC_ROUNDABORT OFF;
GO
IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL AND COL_LENGTH(N'dbo.Orders', N'CheckoutToken') IS NULL
    ALTER TABLE dbo.Orders ADD CheckoutToken UNIQUEIDENTIFIER NULL;
GO
IF OBJECT_ID(N'dbo.Orders', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id=OBJECT_ID(N'dbo.Orders') AND name=N'UX_Orders_User_CheckoutToken')
    CREATE UNIQUE INDEX UX_Orders_User_CheckoutToken ON dbo.Orders(UserId, CheckoutToken) WHERE CheckoutToken IS NOT NULL;
GO
