USE AKPayDB;
GO

-- lastUpdateTime of a USER balance cannot be set to BEFORE the current lastUpdateTime value
CREATE TRIGGER TR_UserAccounts_ValidLastUpdate
ON UserAccounts
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS(
        SELECT 1
        FROM inserted i
        JOIN deleted d ON i.userID = d.userID
        WHERE i.lastUpdateTime < d.lastUpdateTime
    )
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 77000, 'UserAccounts.lastUpdateTime cannot be earlier than existing value', 1;
    END
END;
GO

-- lastUpdateTime of a VENDOR balance cannot be set to BEFORE the current lastUpdateTime value
CREATE TRIGGER TR_Vendors_ValidLastUpdate
ON Vendors
AFTER UPDATE AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS(
        SELECT 1
        FROM inserted i
        JOIN deleted d ON i.vendorID = d.vendorID
        WHERE i.lastUpdateTime < d.lastUpdateTime
    )
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 77001, 'Vendors.lastUpdateTime cannot be earlier than existing value', 1;
    END
END;
GO

-- Block User creation directly
CREATE TRIGGER TR_Users_BlockDirectCreate
ON Users
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        THROW 77002, 'Direct inserts not allowed, use SP_CreateUser', 1;
    END
END
GO

-- Block UserAccount creation directly
CREATE TRIGGER TR_UserAccounts_BlockDirectCreate
ON UserAccounts
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted)
    BEGIN
        THROW 77002, 'Direct inserts not allowed, use SP_CreateUser', 1;
    END
END
GO

-- Block user deletion directly, suggest soft delete sproc
CREATE TRIGGER TR_Users_BlockDirectDelete
ON Users
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        THROW 77002, 'Direct deletes not allowed, use SP_SoftDeleteUser, or if admin-level force delete, use SP_PurgeUser instead', 1;
    END
END
GO

-- Block userAccount delete directly, suggest soft delete sproc
CREATE TRIGGER TR_UserAccounts_BlockDirectDelete
ON UserAccounts
INSTEAD OF DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM deleted)
    BEGIN
        THROW 77003, 'Direct deletes not allowed, use SP_SoftDeleteUser, or if admin-level force delete, use SP_PurgeUser instead', 1;
    END
END
GO

-- lastUpdateTime on UserAccounts
CREATE TRIGGER TR_UserAccounts_lastUpdateTime
ON UserAccounts
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(lastUpdateTime)
        RETURN;

    IF EXISTS(
        SELECT 1
        FROM inserted i
        JOIN deleted d ON i.userID = d.userID
        WHERE i.userBalance <> d.userBalance
            OR i.isActive <> d.isActive
    )
    BEGIN
        UPDATE ua
        SET lastUpdateTime = GETDATE()
        FROM UserAccounts ua
        INNER JOIN inserted i ON ua.userID = i.userID;
    END
END
GO

-- lastUpdateTime on Vendors
CREATE TRIGGER TR_Vendors_lastUpdateTime
ON Vendors
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF UPDATE(lastUpdateTime)
        RETURN;

    IF EXISTS(
        SELECT 1
        FROM inserted i
        JOIN deleted d ON i.vendorID = d.vendorID
        WHERE i.vendorName <> d.vendorName
            OR i.vendorBalance <> d.vendorBalance
            OR i.managerName <> d.managerName
            OR i.managerPhone <> d.managerPhone
            OR i.statusID <> d.statusID
    )
    BEGIN
        UPDATE v
        SET lastUpdateTime = GETDATE()
        FROM Vendors v
        INNER JOIN inserted i ON v.vendorID = i.vendorID;
    END
END
GO

CREATE TRIGGER TR_UserBeneficiaries_nickName
ON UserBeneficiaries
INSTEAD OF INSERT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO UserBeneficiaries(
        remitterID,
        beneficiaryID,
        nickName,
        lastPaymentTime,
        lastPaymentAmount,
        addedAt
    )
    SELECT
        i.remitterID,
        i.beneficiaryID,
        COALESCE(i.nickName, u.fullName),
        i.lastPaymentTime,
        i.lastPaymentAmount,
        COALESCE(i.addedAt, GETDATE())
    FROM
        inserted i
    INNER JOIN
        Users u ON u.userID = i.beneficiaryID;

END;
GO