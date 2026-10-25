-- Run in SQL Server Management Studio before 02, 03, and 04.
-- Never drops or replaces an existing database.
USE [master];
GO

IF DB_ID(N'MetropolitanCoffeeDB') IS NULL
BEGIN
    CREATE DATABASE [MetropolitanCoffeeDB];
END;
GO

USE [MetropolitanCoffeeDB];
GO
