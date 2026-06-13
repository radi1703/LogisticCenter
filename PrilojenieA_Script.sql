IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Couriers] (
    [CourierId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Username] nvarchar(50) NOT NULL,
    [Phone] nvarchar(20) NULL,
    [Zone] nvarchar(50) NULL,
    CONSTRAINT [PK_Couriers] PRIMARY KEY ([CourierId])
);
GO

CREATE TABLE [Roles] (
    [RoleId] int NOT NULL IDENTITY,
    [RoleName] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([RoleId])
);
GO

CREATE TABLE [SystemUsers] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(max) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [Role] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_SystemUsers] PRIMARY KEY ([UserId])
);
GO

CREATE TABLE [Warehouses] (
    [WarehouseId] int NOT NULL IDENTITY,
    [Name] nvarchar(max) NOT NULL,
    [City] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Warehouses] PRIMARY KEY ([WarehouseId])
);
GO

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    [FullName] nvarchar(max) NOT NULL,
    [RoleId] int NULL,
    [CourierId] int NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]),
    CONSTRAINT [FK_Users_Couriers_CourierId] FOREIGN KEY ([CourierId]) REFERENCES [Couriers] ([CourierId]),
    CONSTRAINT [FK_Users_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([RoleId])
);
GO

CREATE TABLE [Shipments] (
    [ShipmentId] int NOT NULL IDENTITY,
    [TrackingNumber] nvarchar(50) NOT NULL,
    [SenderName] nvarchar(max) NOT NULL,
    [SenderPhone] nvarchar(max) NULL,
    [ReceiverName] nvarchar(max) NOT NULL,
    [ReceiverPhone] nvarchar(max) NULL,
    [ReceiverAddress] nvarchar(max) NULL,
    [Weight] decimal(18,2) NULL,
    [Status] nvarchar(max) NULL,
    [CreatedBy] nvarchar(max) NULL,
    [CreatedDate] datetime2 NULL,
    [Note] nvarchar(max) NULL,
    [ProblemType] nvarchar(max) NULL,
    [ProblemDescription] nvarchar(max) NULL,
    [AssignedCourierId] int NULL,
    [CurrentWarehouseId] int NULL,
    CONSTRAINT [PK_Shipments] PRIMARY KEY ([ShipmentId]),
    CONSTRAINT [FK_Shipments_Couriers] FOREIGN KEY ([AssignedCourierId]) REFERENCES [Couriers] ([CourierId]),
    CONSTRAINT [FK_Shipments_Warehouses] FOREIGN KEY ([CurrentWarehouseId]) REFERENCES [Warehouses] ([WarehouseId])
);
GO

CREATE INDEX [IX_Shipments_AssignedCourierId] ON [Shipments] ([AssignedCourierId]);
GO

CREATE INDEX [IX_Shipments_CurrentWarehouseId] ON [Shipments] ([CurrentWarehouseId]);
GO

CREATE INDEX [IX_Users_CourierId] ON [Users] ([CourierId]);
GO

CREATE INDEX [IX_Users_RoleId] ON [Users] ([RoleId]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260612120901_InitialCreate', N'7.0.20');
GO

COMMIT;
GO

