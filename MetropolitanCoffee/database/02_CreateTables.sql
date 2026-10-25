-- MetropolitanCoffeeDB schema. Re-running skips existing tables, keys and indexes.
-- Existing objects with different definitions are not altered automatically.
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

IF OBJECT_ID(N'dbo.Categories', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Categories
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
        Name NVARCHAR(100) COLLATE Latin1_General_100_CI_AI NOT NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Categories_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_Categories_Name UNIQUE (Name),
        CONSTRAINT CK_Categories_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
    );
END;
GO

IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Users
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        Username NVARCHAR(50) COLLATE Latin1_General_100_BIN2 NOT NULL,
        PasswordHash NVARCHAR(255) NOT NULL,
        -- B04 admin/users.php creates a customer without either name.
        -- Empty strings are valid B04 values.
        FirstName NVARCHAR(50) NOT NULL CONSTRAINT DF_Users_FirstName DEFAULT N'',
        LastName NVARCHAR(50) NOT NULL CONSTRAINT DF_Users_LastName DEFAULT N'',
        Email NVARCHAR(100) COLLATE Latin1_General_100_CI_AI NOT NULL,
        Phone NVARCHAR(20) NULL,
        City NVARCHAR(50) NULL,
        Ward NVARCHAR(100) NULL,
        Address NVARCHAR(255) NULL,
        Avatar NVARCHAR(255) NULL,
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT SYSUTCDATETIME(),
        Role NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NOT NULL CONSTRAINT DF_Users_Role DEFAULT N'user',
        Status NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NOT NULL CONSTRAINT DF_Users_Status DEFAULT N'active',
        AuthVersion BIGINT NOT NULL CONSTRAINT DF_Users_AuthVersion DEFAULT (1),
        CONSTRAINT UQ_Users_Username UNIQUE (Username),
        CONSTRAINT UQ_Users_Email UNIQUE (Email),
        CONSTRAINT CK_Users_Username_NotBlank CHECK (LEN(LTRIM(RTRIM(Username))) > 0),
        CONSTRAINT CK_Users_Email_NotBlank CHECK (LEN(LTRIM(RTRIM(Email))) > 0),
        CONSTRAINT CK_Users_PasswordHash_NotBlank CHECK (LEN(PasswordHash) > 0),
        CONSTRAINT CK_Users_Role CHECK (Role IN (N'user', N'admin')),
        CONSTRAINT CK_Users_Status CHECK (Status IN (N'active', N'locked')),
        CONSTRAINT CK_Users_AuthVersion CHECK (AuthVersion BETWEEN 1 AND 4294967295)
    );
END;
GO

-- Add the B04-compatible empty-name defaults when this script is re-run
-- against a schema created before they were introduced.
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.Users') AND parent_column_id = COLUMNPROPERTY(OBJECT_ID(N'dbo.Users'), N'FirstName', 'ColumnId'))
    ALTER TABLE dbo.Users ADD CONSTRAINT DF_Users_FirstName DEFAULT N'' FOR FirstName;
IF OBJECT_ID(N'dbo.Users', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.default_constraints WHERE parent_object_id = OBJECT_ID(N'dbo.Users') AND parent_column_id = COLUMNPROPERTY(OBJECT_ID(N'dbo.Users'), N'LastName', 'ColumnId'))
    ALTER TABLE dbo.Users ADD CONSTRAINT DF_Users_LastName DEFAULT N'' FOR LastName;
GO

IF OBJECT_ID(N'dbo.Products', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Products
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
        CategoryId INT NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        ShortDescription NVARCHAR(255) NULL,
        Description NVARCHAR(MAX) NULL,
        Ingredients NVARCHAR(MAX) NULL,
        Image NVARCHAR(255) NULL,
        CostPrice DECIMAL(10,2) NOT NULL CONSTRAINT DF_Products_CostPrice DEFAULT (0),
        ProfitPercent DECIMAL(5,2) NOT NULL CONSTRAINT DF_Products_ProfitPercent DEFAULT (0),
        SellPrice AS CONVERT(DECIMAL(10,2), ROUND(CostPrice * (1 + ProfitPercent / CONVERT(DECIMAL(5,2), 100)), 2)) PERSISTED,
        Quantity INT NOT NULL CONSTRAINT DF_Products_Quantity DEFAULT (0),
        Status NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NOT NULL CONSTRAINT DF_Products_Status DEFAULT N'active',
        HasSize BIT NOT NULL CONSTRAINT DF_Products_HasSize DEFAULT (1),
        HasIce BIT NOT NULL CONSTRAINT DF_Products_HasIce DEFAULT (1),
        HasSugar BIT NOT NULL CONSTRAINT DF_Products_HasSugar DEFAULT (1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Products_Name_NotBlank CHECK (LEN(LTRIM(RTRIM(Name))) > 0),
        CONSTRAINT CK_Products_CostPrice CHECK (CostPrice >= 0),
        CONSTRAINT CK_Products_ProfitPercent CHECK (ProfitPercent >= 0),
        CONSTRAINT CK_Products_Quantity CHECK (Quantity >= 0),
        CONSTRAINT CK_Products_Status CHECK (Status IN (N'active', N'inactive'))
    );
END;
GO

IF OBJECT_ID(N'dbo.Orders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Orders
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
        UserId INT NOT NULL,
        ReceiverName NVARCHAR(100) NULL,
        ReceiverPhone NVARCHAR(20) NULL,
        ShipCity NVARCHAR(50) NULL,
        ShipWard NVARCHAR(100) NULL,
        ShipAddress NVARCHAR(255) NULL,
        Note NVARCHAR(255) NULL,
        Total DECIMAL(10,2) NOT NULL,
        PaymentMethod NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NOT NULL,
        Status NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NOT NULL CONSTRAINT DF_Orders_Status DEFAULT N'pending',
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT SYSUTCDATETIME(),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Orders_UpdatedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT CK_Orders_Total CHECK (Total >= 0),
        CONSTRAINT CK_Orders_PaymentMethod CHECK (PaymentMethod IN (N'cash', N'bank', N'online')),
        CONSTRAINT CK_Orders_Status CHECK (Status IN (N'pending', N'confirmed', N'shipping', N'delivered', N'cancelled'))
    );
END;
GO

IF OBJECT_ID(N'dbo.OrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.OrderItems
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_OrderItems PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL,
        Price DECIMAL(10,2) NOT NULL, -- immutable sale price at checkout
        Size NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NULL,
        Ice NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NULL,
        Sugar NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NULL,
        CONSTRAINT UQ_OrderItems_Id_ProductId UNIQUE (Id, ProductId),
        CONSTRAINT CK_OrderItems_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_OrderItems_Price CHECK (Price >= 0),
        CONSTRAINT CK_OrderItems_Size CHECK (Size IS NULL OR Size IN (N'none', N'S', N'M', N'L')),
        CONSTRAINT CK_OrderItems_Ice CHECK (Ice IS NULL OR Ice IN (N'none', N'less', N'normal', N'more')),
        CONSTRAINT CK_OrderItems_Sugar CHECK (Sugar IS NULL OR Sugar IN (N'none', N'less', N'normal', N'more'))
    );
END;
GO

IF OBJECT_ID(N'dbo.PurchaseOrders', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseOrders
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PurchaseOrders PRIMARY KEY,
        CreatedAt DATE NOT NULL CONSTRAINT DF_PurchaseOrders_CreatedAt DEFAULT CONVERT(DATE, SYSUTCDATETIME()),
        Status NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NOT NULL CONSTRAINT DF_PurchaseOrders_Status DEFAULT N'pending',
        CONSTRAINT CK_PurchaseOrders_Status CHECK (Status IN (N'pending', N'completed'))
    );
END;
GO

IF OBJECT_ID(N'dbo.PurchaseOrderItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.PurchaseOrderItems
    (
        Id INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_PurchaseOrderItems PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        ImportPrice DECIMAL(10,2) NOT NULL, -- immutable receipt price
        Quantity INT NOT NULL,
        CONSTRAINT UQ_PurchaseOrderItems_Id_ProductId UNIQUE (Id, ProductId),
        CONSTRAINT CK_PurchaseOrderItems_ImportPrice CHECK (ImportPrice >= 0),
        CONSTRAINT CK_PurchaseOrderItems_Quantity CHECK (Quantity > 0)
    );
END;
GO

-- Relational replacement for users.cart_data. Quantity zero preserves sold-out cart items.
IF OBJECT_ID(N'dbo.CartItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CartItems
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_CartItems PRIMARY KEY,
        UserId INT NOT NULL,
        ProductId INT NOT NULL,
        Size NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NOT NULL CONSTRAINT DF_CartItems_Size DEFAULT N'none',
        Ice NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NOT NULL CONSTRAINT DF_CartItems_Ice DEFAULT N'none',
        Sugar NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NOT NULL CONSTRAINT DF_CartItems_Sugar DEFAULT N'none',
        Quantity INT NOT NULL,
        AddedAt DATETIME2(0) NOT NULL CONSTRAINT DF_CartItems_AddedAt DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_CartItems_User_Product_Options UNIQUE (UserId, ProductId, Size, Ice, Sugar),
        CONSTRAINT CK_CartItems_Quantity CHECK (Quantity >= 0),
        CONSTRAINT CK_CartItems_Size CHECK (Size IN (N'none', N'S', N'M', N'L')),
        CONSTRAINT CK_CartItems_Ice CHECK (Ice IN (N'none', N'less', N'normal', N'more')),
        CONSTRAINT CK_CartItems_Sugar CHECK (Sugar IN (N'none', N'less', N'normal', N'more'))
    );
END;
GO

-- Audit trail; Products.Quantity is the fast current balance.
-- Services must update both within the same transaction.
IF OBJECT_ID(N'dbo.StockMovements', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.StockMovements
    (
        Id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_StockMovements PRIMARY KEY,
        ProductId INT NOT NULL,
        MovementType NVARCHAR(20) COLLATE Latin1_General_100_BIN2 NOT NULL,
        QuantityDelta INT NOT NULL,
        OrderItemId INT NULL,
        PurchaseOrderItemId INT NULL,
        OccurredAt DATETIME2(0) NOT NULL CONSTRAINT DF_StockMovements_OccurredAt DEFAULT SYSUTCDATETIME(),
        Note NVARCHAR(255) NULL,
        CONSTRAINT CK_StockMovements_QuantityDelta CHECK (QuantityDelta <> 0),
        CONSTRAINT CK_StockMovements_SourceAndSign CHECK (
            (MovementType = N'purchase' AND QuantityDelta > 0 AND PurchaseOrderItemId IS NOT NULL AND OrderItemId IS NULL)
            OR (MovementType = N'sale' AND QuantityDelta < 0 AND OrderItemId IS NOT NULL AND PurchaseOrderItemId IS NULL)
            OR (MovementType = N'sale_cancel' AND QuantityDelta > 0 AND OrderItemId IS NOT NULL AND PurchaseOrderItemId IS NULL)
            OR (MovementType = N'opening' AND QuantityDelta > 0 AND OrderItemId IS NULL AND PurchaseOrderItemId IS NULL)
            OR (MovementType = N'adjust_in' AND QuantityDelta > 0 AND OrderItemId IS NULL AND PurchaseOrderItemId IS NULL AND Note IS NOT NULL AND LEN(LTRIM(RTRIM(Note))) > 0)
            OR (MovementType = N'adjust_out' AND QuantityDelta < 0 AND OrderItemId IS NULL AND PurchaseOrderItemId IS NULL AND Note IS NOT NULL AND LEN(LTRIM(RTRIM(Note))) > 0)
        )
    );
END;
GO

-- Explicit NO ACTION on all relationships protects order and stock history.
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Products_Categories' AND parent_object_id = OBJECT_ID(N'dbo.Products'))
    ALTER TABLE dbo.Products ADD CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(Id) ON DELETE NO ACTION;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Orders_Users' AND parent_object_id = OBJECT_ID(N'dbo.Orders'))
    ALTER TABLE dbo.Orders ADD CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE NO ACTION;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_OrderItems_Orders' AND parent_object_id = OBJECT_ID(N'dbo.OrderItems'))
    ALTER TABLE dbo.OrderItems ADD CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(Id) ON DELETE NO ACTION;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_OrderItems_Products' AND parent_object_id = OBJECT_ID(N'dbo.OrderItems'))
    ALTER TABLE dbo.OrderItems ADD CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE NO ACTION;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PurchaseOrderItems_PurchaseOrders' AND parent_object_id = OBJECT_ID(N'dbo.PurchaseOrderItems'))
    ALTER TABLE dbo.PurchaseOrderItems ADD CONSTRAINT FK_PurchaseOrderItems_PurchaseOrders FOREIGN KEY (OrderId) REFERENCES dbo.PurchaseOrders(Id) ON DELETE NO ACTION;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_PurchaseOrderItems_Products' AND parent_object_id = OBJECT_ID(N'dbo.PurchaseOrderItems'))
    ALTER TABLE dbo.PurchaseOrderItems ADD CONSTRAINT FK_PurchaseOrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE NO ACTION;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CartItems_Users' AND parent_object_id = OBJECT_ID(N'dbo.CartItems'))
    ALTER TABLE dbo.CartItems ADD CONSTRAINT FK_CartItems_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE NO ACTION;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_CartItems_Products' AND parent_object_id = OBJECT_ID(N'dbo.CartItems'))
    ALTER TABLE dbo.CartItems ADD CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE NO ACTION;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StockMovements_Products' AND parent_object_id = OBJECT_ID(N'dbo.StockMovements'))
    ALTER TABLE dbo.StockMovements ADD CONSTRAINT FK_StockMovements_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id) ON DELETE NO ACTION;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StockMovements_OrderItems' AND parent_object_id = OBJECT_ID(N'dbo.StockMovements'))
    ALTER TABLE dbo.StockMovements ADD CONSTRAINT FK_StockMovements_OrderItems FOREIGN KEY (OrderItemId, ProductId) REFERENCES dbo.OrderItems(Id, ProductId) ON DELETE NO ACTION;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_StockMovements_PurchaseOrderItems' AND parent_object_id = OBJECT_ID(N'dbo.StockMovements'))
    ALTER TABLE dbo.StockMovements ADD CONSTRAINT FK_StockMovements_PurchaseOrderItems FOREIGN KEY (PurchaseOrderItemId, ProductId) REFERENCES dbo.PurchaseOrderItems(Id, ProductId) ON DELETE NO ACTION;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Products') AND name = N'IX_Products_Category_Status_Name')
    CREATE INDEX IX_Products_Category_Status_Name ON dbo.Products(CategoryId, Status, Name);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Orders') AND name = N'IX_Orders_User_CreatedAt')
    CREATE INDEX IX_Orders_User_CreatedAt ON dbo.Orders(UserId, CreatedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.Orders') AND name = N'IX_Orders_Status_UpdatedAt')
    CREATE INDEX IX_Orders_Status_UpdatedAt ON dbo.Orders(Status, UpdatedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OrderItems') AND name = N'IX_OrderItems_OrderId')
    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.OrderItems') AND name = N'IX_OrderItems_ProductId')
    CREATE INDEX IX_OrderItems_ProductId ON dbo.OrderItems(ProductId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.PurchaseOrders') AND name = N'IX_PurchaseOrders_Status_CreatedAt')
    CREATE INDEX IX_PurchaseOrders_Status_CreatedAt ON dbo.PurchaseOrders(Status, CreatedAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.PurchaseOrderItems') AND name = N'IX_PurchaseOrderItems_OrderId')
    CREATE INDEX IX_PurchaseOrderItems_OrderId ON dbo.PurchaseOrderItems(OrderId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.PurchaseOrderItems') AND name = N'IX_PurchaseOrderItems_ProductId')
    CREATE INDEX IX_PurchaseOrderItems_ProductId ON dbo.PurchaseOrderItems(ProductId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.CartItems') AND name = N'IX_CartItems_ProductId')
    CREATE INDEX IX_CartItems_ProductId ON dbo.CartItems(ProductId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.StockMovements') AND name = N'IX_StockMovements_Product_OccurredAt')
    CREATE INDEX IX_StockMovements_Product_OccurredAt ON dbo.StockMovements(ProductId, OccurredAt DESC);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.StockMovements') AND name = N'UX_StockMovements_OrderItem_Type')
    CREATE UNIQUE INDEX UX_StockMovements_OrderItem_Type ON dbo.StockMovements(OrderItemId, MovementType) WHERE OrderItemId IS NOT NULL;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID(N'dbo.StockMovements') AND name = N'UX_StockMovements_PurchaseOrderItem_Type')
    CREATE UNIQUE INDEX UX_StockMovements_PurchaseOrderItem_Type ON dbo.StockMovements(PurchaseOrderItemId, MovementType) WHERE PurchaseOrderItemId IS NOT NULL;
GO





