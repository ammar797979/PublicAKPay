USE AKPayDB;
GO

-- Soft deletion of users (no deletion allowed on users, just set it to
-- isDeleted and no deletion allowed on userAccount, just set it to inactive)
CREATE PROCEDURE SP_SoftDeleteUser
    @userID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Users WHERE userID = @userID)
            THROW 75000, 'User not found', 1;
        IF EXISTS (SELECT 1 FROM Users WHERE userID = @userID AND isDeleted = 1)
            THROW 75001, 'User is already deleted', 1;
        IF EXISTS (
            SELECT 1 FROM RegularTransactions
            WHERE fromUserID = @userID AND txStatusID IN (1, 2, 4) -- Pending, Sync Pending, Sync Failed
        )
            THROW 75002, 'Cannot delete user with unsettled regular transactions', 1;
        IF EXISTS (
            SELECT 1 FROM TopUpTransactions
            WHERE toUserID = @userID AND txStatusID IN (1, 2, 4)
        )
            THROW 75003, 'Cannot delete user with unsettled top-up transactions', 1;
        IF EXISTS (
            SELECT 1 FROM UserToUserTransactions
            WHERE (toUserID = @userID OR fromUserID = @userID) AND txStatusID IN (1, 2, 4)
        )
            THROW 75004, 'Cannot delete sure with unsettled user-to-user transactions', 1;
        IF EXISTS (
            SELECT 1 FROM UserAccounts
            WHERE userID = @userID AND userBalance > 0
        )
            THROW 75005, 'Cannot delete user who has balance in account', 1;

        UPDATE Users
        SET isDeleted = 1, deletedAt = GETDATE()
        WHERE userID = @userID;

        UPDATE UserAccounts
        SET isActive = 0, lastUpdateTime = GETDATE()
        WHERE userID = @userID;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Admin only sproc, to physically delete a user from the tables if needed
CREATE PROCEDURE SP_PurgeUser
    @userID INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM Users WHERE userID = @userID)
            THROW 75006, 'User not found', 1;
        IF EXISTS (
            SELECT 1 FROM RegularTransactions
            WHERE fromUserID = @userID AND txStatusID IN (1, 2, 4) -- Pending, Sync Pending, Sync Failed
        )
            THROW 75007, 'Cannot purge user with unsettled regular transactions', 1;
        IF EXISTS (
            SELECT 1 FROM TopUpTransactions
            WHERE toUserID = @userID AND txStatusID IN (1, 2 ,4)
        )
            THROW 75008, 'Cannot purge user with unsettled top-up transactions', 1;
        IF EXISTS (
            SELECT 1 FROM UserToUserTransactions
            WHERE (toUserID = @userID OR fromUserID = @userID) AND txStatusID IN (1, 2, 4)
        )
            THROW 75009, 'Cannot purge user with unsettled user-to-user transactions', 1;
        IF EXISTS (
            SELECT 1 FROM UserAccounts
            WHERE userID = @userID AND userBalance > 0
        )
            THROW 75010, 'Cannot purge user who we owe monies', 1;

        -- Disable triggers to allow sproc to delete
        ALTER TABLE UserAccounts DISABLE TRIGGER TR_UserAccounts_BlockDirectDelete;
        ALTER TABLE Users DISABLE TRIGGER TR_Users_BlockDirectDelete;

        DELETE FROM UserAccounts WHERE userID = @userID;
        DELETE FROM Users WHERE userID = @userID;

        -- Re-enable triggers
        ALTER TABLE UserAccounts ENABLE TRIGGER TR_UserAccounts_BlockDirectDelete;
        ALTER TABLE Users ENABLE TRIGGER TR_Users_BlockDirectDelete;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO

-- Create user sproc, maintains 1-to-1 relationship with userAccounts by creating an account for that user too automatically
CREATE PROCEDURE SP_CreateUser
    @email       VARCHAR(100),
    @phone       VARCHAR(11),
    @fullName    VARCHAR(100),
    @passwordHash VARCHAR(255),
    @userType    VARCHAR(20) = NULL,
    @userID INT = NULL OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM Users WHERE email = @email AND isDeleted = 0)
            THROW 75011, 'An active user with this email already exists.', 1;

        IF EXISTS (SELECT 1 FROM Users WHERE phone = @phone AND isDeleted = 0)
            THROW 75012, 'Phone number already in use by an active user.', 1;

        IF ISNULL(@fullName, '') NOT LIKE '%[^ ]%'
            THROW 75013, 'Full Name cannot be empty', 1;

        -- Disable triggers to allow sproc to insert
        ALTER TABLE Users DISABLE TRIGGER TR_Users_BlockDirectCreate;
        ALTER TABLE UserAccounts DISABLE TRIGGER TR_UserAccounts_BlockDirectCreate;

        INSERT INTO Users (email, phone, fullName, passwordHash, userType)
        VALUES (@email, @phone, @fullName, @passwordHash, @userType);

        SET @userID = CAST(SCOPE_IDENTITY() AS INT);

        INSERT INTO UserAccounts (userID)
        VALUES (@userID);

        -- Re-enable triggers
        ALTER TABLE Users ENABLE TRIGGER TR_Users_BlockDirectCreate;
        ALTER TABLE UserAccounts ENABLE TRIGGER TR_UserAccounts_BlockDirectCreate;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO