-- Login
CREATE OR ALTER PROCEDURE dbo.LoginUser
    @Email VARCHAR(100),
    @PasswordHash VARCHAR(255),
    @result INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserID INT;
    DECLARE @IsDeleted BIT;

    SELECT
        @UserID = userID,
        @IsDeleted = isDeleted
    FROM Users
    WHERE email = @Email
        AND passwordHash = @PasswordHash;

    IF @UserID IS NULL
        BEGIN
            SET @result = -1;
            RETURN;
        END

    IF @IsDeleted = 1
        BEGIN
            SET @result = -2;
            RETURN;
        END

    SET @result = 0;
            
    SELECT
        userID,
        email,
        phone,
        fullName,
        userName,
        userType,
        dateCreated,
        isDeleted
    FROM Users
    WHERE userID = @UserID;

    RETURN @result;
END
GO

-- User Registration Check
CREATE OR ALTER PROCEDURE dbo.IsUserRegistered
    @Email VARCHAR(100),
    @result INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @result = 0;

    SELECT TOP 1 @result =
        CASE
            WHEN isDeleted = 0 THEN 1
            ELSE -1
        END
    FROM Users
    WHERE email = @Email;

    RETURN @result;
END
GO

-- Reset Password
CREATE OR ALTER PROCEDURE dbo.ChangePassword
    @Email VARCHAR(100),
    @NewPasswordHash VARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @UserID INT;
    SELECT @UserID = userID
    FROM Users
    WHERE email = @Email AND isDeleted = 0;

    IF @UserID IS NULL
    BEGIN
        RAISERROR('User not found or is inactive', 16, 1);
        RETURN;
    END
    
    BEGIN TRY
        BEGIN TRAN;

        UPDATE Users
        SET passwordHash = @NewPasswordHash
        WHERE email = @Email
            AND isDeleted = 0;

        IF @@ROWCOUNT = 0
            THROW 51002, 'Password update failed, user record missing', 1;

        COMMIT TRAN;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO

-- User Validation
CREATE OR ALTER PROCEDURE dbo.ValidateUser
    @UserID INT
AS
BEGIN  
    IF NOT EXISTS(SELECT 1 FROM dbo.Users WHERE userID = @UserID AND isDeleted = 0)
    THROW 50005, 'Unvalid user.', 1;
    RETURN 0;
END
GO

-- Validate vendor
CREATE OR ALTER PROCEDURE dbo.ValidateVendor
    @VendorID INT
AS
BEGIN  
    IF NOT EXISTS(SELECT 1 FROM dbo.Vendors WHERE vendorID = @VendorID)
    THROW 50006, 'Unvalid vendor.', 1;
    RETURN 0;
END
GO

-- Validate Source
CREATE OR ALTER PROCEDURE dbo.ValidateTopUpSource
    @SourceID INT
AS
BEGIN  
    IF NOT EXISTS(SELECT 1 FROM dbo.TopUpSources WHERE sourceID = @SourceID)
    THROW 50007, 'Unvalid topup source.', 1;
    RETURN 0;
END
GO

--1. TopUp TX
CREATE OR ALTER PROCEDURE dbo.TopUp_tx
    @SourceID INT,
    @ToUserID INT,
    @Amount DECIMAL (20,2)
AS
BEGIN
    SET XACT_ABORT ON;

    EXEC dbo.ValidateTopUpSource @SourceID;
    EXEC dbo.ValidateUser @ToUserID;

    BEGIN TRY
        BEGIN TRAN;

        SELECT userBalance
        FROM dbo.UserAccounts WITH (ROWLOCK, UPDLOCK, HOLDLOCK)
        WHERE userID = @ToUserID;

        UPDATE dbo.UserAccounts
        SET userBalance = userBalance + @Amount,
            lastUpdateTime = GETDATE()
        WHERE userID = @ToUserID


        INSERT INTO dbo.TopUpTransactions (sourceID, toUserID, amount, txTimeStamp)
        VALUES (@SourceID, @ToUserID, @Amount, GETDATE());

        COMMIT TRAN;
        RETURN 0;
    END TRY

    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;
        THROW
    END CATCH
END
GO

-- U2U INTERFACE
-- SEARCH
CREATE OR ALTER PROCEDURE dbo.SearchUser
    @SearchWord VARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        userID,
        email,
        phone,
        fullName,
        userName,
        userType,
        isDeleted,
        '' AS passwordHash,
        GETDATE() AS dateCreated,
        NULL deletedAt
    FROM Users u
    WHERE isDeleted = 0
        AND userName = @SearchWord
    ORDER BY userID;

    RETURN 0;
END
GO

-- ADD BENEFIICIARY
CREATE OR ALTER PROCEDURE dbo.AddBeneficiary
    @RemitterID INT,
    @BeneficiaryUsername VARCHAR(100),
    @Nickname VARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ResolvedBeneficiaryID INT;

    SELECT @ResolvedBeneficiaryID = userID
    FROM Users
    WHERE userName = @BeneficiaryUsername
        AND isDeleted = 0;

    IF @ResolvedBeneficiaryID IS NULL
        THROW 51004, 'Beneficiary username not found or is inactive.', 1;

    EXEC dbo.ValidateUser @RemitterID;

    IF @RemitterID = @ResolvedBeneficiaryID
        THROW 51002, 'Cannot add yourself as a beneficiary', 1;

    IF EXISTS(SELECT 1 FROM UserBeneficiaries
            WHERE remitterID = @RemitterID
            AND beneficiaryID = @ResolvedBeneficiaryID)
        THROW 51003, 'Beneficiary already exists', 1;

    BEGIN TRY
        INSERT INTO UserBeneficiaries (remitterID, beneficiaryID, nickName)
        VALUES (@RemitterID, @ResolvedBeneficiaryID, @Nickname);

        RETURN 0;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END
GO

-- LIST BENEFICIARY
CREATE OR ALTER PROCEDURE dbo.ListBeneficiaries
    @RemitterID INT
AS
BEGIN
    SET NOCOUNT ON;

    EXEC dbo.ValidateUser @RemitterID;

    SELECT
        ub.nickName AS NickName,
        u.fullName AS BeneficiaryFullName,
        u.userName AS UserName
    FROM UserBeneficiaries ub
    JOIN Users u ON ub.beneficiaryID = u.userID
    WHERE ub.remitterID = @RemitterID
    ORDER BY ub.lastPaymentTime DESC, ub.nickName ASC;

END
GO
    
--2. U2U Transfer
CREATE OR ALTER PROCEDURE dbo.U2U_tx
    @FromUserID INT,
    @ToUserID INT,
    @Amount DECIMAL (20,2)

AS 
BEGIN
    SET XACT_ABORT ON;

    IF @FromUserID = @ToUserID
        THROW 50001, 'Sender and receiver cannot be the same.', 1;

    BEGIN TRY

        EXEC dbo.ValidateUser @FromUserID;
        EXEC dbo.ValidateUser @ToUserID;

        IF NOT EXISTS(SELECT 1 FROM UserBeneficiaries 
                WHERE remitterID = @FromUserID 
                AND beneficiaryID = @ToUserID)
        THROW 51006, 'Cannot find beneficiary user.', 1;
        
        BEGIN TRAN;

        IF @FromUserID < @ToUserID
        BEGIN
        SELECT userBalance FROM dbo.UserAccounts WITH (ROWLOCK, UPDLOCK, HOLDLOCK) WHERE userID = @FromUserID;
        SELECT userBalance FROM dbo.UserAccounts WITH (ROWLOCK, UPDLOCK, HOLDLOCK) WHERE userID = @ToUserID;
        END
        ELSE
        BEGIN
        SELECT userBalance FROM dbo.UserAccounts WITH (ROWLOCK, UPDLOCK, HOLDLOCK) WHERE userID = @ToUserID;
        SELECT userBalance FROM dbo.UserAccounts WITH (ROWLOCK, UPDLOCK, HOLDLOCK) WHERE userID = @FromUserID;
        END

        DECLARE @FromBalance DECIMAL (20,2);
        SELECT @FromBalance = userBalance FROM dbo.UserAccounts WHERE userID = @FromUserID;

        IF @FromBalance < @Amount
        BEGIN
            ROLLBACK TRAN;
        THROW 50002, 'Insufficient balance.', 1;
        END

        UPDATE dbo.UserAccounts
        SET userBalance = userBalance - @Amount,
            lastUpdateTime = GETDATE()
        WHERE userID = @FromUserID;

        UPDATE dbo.UserAccounts
        SET userBalance = userBalance + @Amount,
            lastUpdateTime = GETDATE()
        WHERE userID = @ToUserID;

        INSERT INTO UserToUserTransactions (toUserID, fromUserID, amount, txTimeStamp)
        VALUES (@ToUserID, @FromUserID, @Amount, GETDATE());

        UPDATE UserBeneficiaries
                SET lastPaymentTime = GETDATE(),
                    lastPaymentAmount = @Amount
                WHERE remitterID = @FromUserID
                    AND beneficiaryID = @ToUserID;

        COMMIT TRAN;
        RETURN 0;
    END TRY

    BEGIN CATCH
    IF @@TRANCOUNT > 0
            ROLLBACK TRAN;
        THROW
    END CATCH
END
GO

--3. Regular_tx
CREATE OR ALTER PROCEDURE dbo.Regular_tx
    @UserID INT,
    @VendorID INT,
    @Amount DECIMAL(18,2),
    @IsOnline BIT = 0
AS
BEGIN
    SET XACT_ABORT ON;

    DECLARE @PaymentMode NVARCHAR(10);

    IF @IsOnline = 1
        SET @PaymentMode = 'Online';
    ELSE
        SET @PaymentMode = 'Offline';

    EXEC dbo.ValidateUser @UserID;
    EXEC dbo.ValidateVendor @VendorID;

BEGIN TRY
    
    EXEC dbo.ValidateUser @UserID;
    EXEC dbo.ValidateVendor @VendorID;

    BEGIN TRAN;

    IF @UserID < @VendorID
    BEGIN
    SELECT userBalance FROM dbo.UserAccounts WITH (ROWLOCK, UPDLOCK, HOLDLOCK) WHERE userID = @UserID;
    SELECT vendorBalance FROM dbo.Vendors WITH (ROWLOCK, UPDLOCK, HOLDLOCK) WHERE vendorID = @VendorID;
    END
    ELSE
    BEGIN
    SELECT vendorBalance FROM dbo.Vendors WITH (ROWLOCK, UPDLOCK, HOLDLOCK) WHERE vendorID = @VendorID;
    SELECT userBalance FROM dbo.UserAccounts WITH (ROWLOCK, UPDLOCK, HOLDLOCK) WHERE userID = @UserID;
    END

    DECLARE @UserBal DECIMAL (20,2);
    SELECT @UserBal = userBalance FROM dbo.UserAccounts WHERE userID = @UserID;

    IF @UserBal < @Amount
        BEGIN
            ROLLBACK TRAN;
            THROW 50003, 'Insufficient balance.', 1;
        END

    UPDATE dbo.UserAccounts
    SET userBalance = userBalance - @Amount,
        lastUpdateTime = GETDATE()
    WHERE userID = @UserID;

    UPDATE dbo.Vendors
    SET vendorBalance = vendorBalance + @Amount,
        lastUpdateTime = GETDATE()
    WHERE vendorID = @VendorID;

    INSERT INTO dbo.RegularTransactions (fromUserID, toVendorID, amount, txTimeStamp, paymentMode)
    VALUES (@UserID, @VendorID, @Amount, GETDATE(), @PaymentMode);

    COMMIT TRAN;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO

--4. VendorPay
CREATE OR ALTER PROCEDURE dbo.VendorPay_tx
    @SourceID INT,
    @VendorID INT,
    @Amount DECIMAL (20,2)
AS
BEGIN
    SET XACT_ABORT ON;

    EXEC dbo.ValidateTopUpSource @SourceID;

    IF @SourceID <> 1
        THROW 50004, 'Only Admin can VendorPay.', 1;

    EXEC dbo.ValidateVendor @VendorID;

    BEGIN TRY
        BEGIN TRAN;

        SELECT vendorBalance
        FROM dbo.Vendors WITH (ROWLOCK, UPDLOCK, HOLDLOCK)
        WHERE vendorID = @VendorID;

        UPDATE dbo.Vendors
        SET vendorBalance = vendorBalance + @Amount,
            lastUpdateTime = GETDATE()
        WHERE vendorID = @VendorID;

        INSERT INTO dbo.VendorPaymentTransaction (toVendorID, amount, txTimeStamp)
        VALUES(@VendorID, @Amount, GETDATE());

        COMMIT TRAN;
        RETURN 0;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
        ROLLBACK TRAN;
        THROW;
    END CATCH
END
GO

-- TX Handling
CREATE OR ALTER PROCEDURE dbo.TX_Handling
    @FromID INT = NULL,
    @ToID INT = NULL,
    @Amount DECIMAL (20,2),
    @TransactionType NVARCHAR(50),
    @ExtraSourceID INT = NULL,
    @PaymentMode NVARCHAR(10) = NULL
AS
BEGIN
    SET XACT_ABORT ON;

    IF @TransactionType = 'TopUp'
    BEGIN
        EXEC dbo.TopUp_tx
            @ToUserID = @ToID,
            @Amount = @Amount,
            @SourceID = @ExtraSourceID;
        RETURN 0;
    END
    ELSE IF @TransactionType = 'U2U'
    BEGIN
        EXEC dbo.U2U_tx
            @FromUserID = @FromID,
            @ToUserID = @ToID,
            @Amount = @Amount;
        RETURN 0;
    END
    ELSE IF @TransactionType = 'Regular'
    BEGIN
        DECLARE @IsOnline BIT = 0;

        IF @PaymentMode IS NOT NULL AND LOWER(@PaymentMode) = 'online'
            SET @IsOnline = 1;

        EXEC dbo.Regular_tx
            @UserID = @FromID,
            @VendorID = @ToID,
            @Amount = @Amount;
        RETURN 0;
    END
    ELSE IF @TransactionType = 'VendorPay'
    BEGIN
        EXEC dbo.VendorPay_tx
            @SourceID = @FromID,
            @VendorID = @ToID,
            @Amount = @Amount;
        RETURN 0;
    END
    ELSE
        THROW 50000, 'Invalid transaction.', 1;
END
GO

-- TX History
CREATE OR ALTER PROCEDURE GetUserTransactionHistory
    @UserID INT
AS
BEGIN
    SET NOCOUNT ON;

    --U2U
    SELECT
        'U2U' as txType,
        fromUserID AS senderID,
        dbo.GetSenderName('U2U', UToUTransactionID) AS senderName,
        toUserID AS receiverID,
        dbo.GetReceiverName('U2U', UToUTransactionID) AS receiverName,
        amount,
        txTimeStamp,
        NULL AS paymentMode,
        dbo.GetTXSign('U2U', UToUTransactionID, @UserID, 'User') AS sign
    FROM UserToUserTransactions
    WHERE fromUserID = @UserID OR toUserID = @UserID

    UNION ALL

    -- Regular
    SELECT
        'Regular' as txType,
        fromUserID AS senderID,
        dbo.GetSenderName('Regular', regularTransactionID) AS senderName,
        toVendorID AS receiverID,
        dbo.GetReceiverName('Regular', regularTransactionID) AS receiverName,
        amount,
        txTimeStamp,
        paymentMode,
        dbo.GetTXSign('Regular', regularTransactionID, @UserID, 'User') AS sign
    FROM RegularTransactions
    WHERE fromUserID = @UserID

    UNION ALL

    SELECT
        'TopUp' AS txType,
        NULL AS senderID,
        dbo.GetSenderName('TopUp', topUpTransactionID) AS senderName,
        toUserID AS receiverID,
        dbo.GetReceiverName('TopUp', topUpTransactionID) AS receiverName,
        amount,
        txTimeStamp,
        NULL AS paymentMode,
        dbo.GetTXSign('TopUp', topUpTransactionID, @UserID, 'User') AS sign
    FROM TopUpTransactions t
    WHERE toUserID = @UserID

    ORDER BY txTimeStamp DESC;
END
GO

CREATE OR ALTER PROCEDURE GetVendorTransactionHistory
    @VendorID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        'Regular' AS txType,
        t.fromUserID AS senderID,
        u.fullName AS senderName,
        t.toVendorID AS receiverID,
        v.vendorName AS receiverName,
        t.amount,
        t.txTimeStamp,
        t.paymentMode,
        dbo.GetTXSign('Regular', regularTransactionID, @VendorID, 'Vendor') AS sign
    FROM RegularTransactions t
    JOIN Users u ON t.fromUserID = u.userID
    JOIN Vendors v ON t.toVendorID = v.vendorID
    WHERE t.toVendorID = @VendorID

    UNION ALL

    SELECT
        'VendorPay' AS txType,
        NULL AS senderID,
        'Admin' AS senderName,
        t.toVendorID AS receiverID,
        v.vendorName AS receiverName,
        t.amount,
        t.txTimeStamp,
        'Online' AS paymentMode,
        dbo.GetTXSign('VendorPay', vendorPaymentTransactionID, @VendorID, 'Vendor') AS sign
    FROM VendorPaymentTransaction t
    JOIN Vendors v ON t.toVendorID = v.vendorID
    WHERE t.toVendorID = @VendorID

    ORDER BY txTimeStamp DESC;
END
GO

-- Notif Log
CREATE OR ALTER PROCEDURE dbo.LogNotifications
    @RecipientType VARCHAR(10),
    @RecipientID INT,
    @Msg VARCHAR(255),
    @NotifType VARCHAR(10),
    @TxType VARCHAR(20) = NULL,
    @TxID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @RecipientType = 'User' AND NOT EXISTS (
        SELECT 1 FROM Users WHERE userID = @RecipientID
            AND isDeleted = 0
    )
    RETURN;

    IF @RecipientType = 'Vendor' AND NOT EXISTS (
        SELECT 1 FROM Vendors WHERE vendorID = @RecipientID
    )
    RETURN;

    BEGIN TRY
        INSERT INTO Notifications (recipientType, recipientID, msg, notifType, txType, txID)
        VALUES (@RecipientType, @RecipientID, @Msg, @NotifType, @TxType, @TxID);

        RETURN 0;
    END TRY
    BEGIN CATCH
        RETURN -1;
    END CATCH

END
GO
