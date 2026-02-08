-- FUNC TO GET COUNT PER TRANSACTION TYPE
CREATE OR ALTER FUNCTION dbo.GetTransactionCount(
    @UserID INT,
    @TxType NVARCHAR(20)
)
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;

    IF @TxType = 'U2U'
    BEGIN
        SELECT @Count = COUNT(*)
        FROM dbo.UserToUserTransactions
        WHERE fromUserID = @UserID 
            OR toUserID = @UserID;
    END
    ELSE IF @TxType = 'Regular'
    BEGIN
        SELECT @Count = COUNT(*)
        FROM dbo.RegularTransactions
        WHERE fromUserID = @UserID;
    END
    ELSE IF @TxType = 'TopUp'
    BEGIN
        SELECT @Count = COUNT(*)
        FROM dbo.TopUpTransactions
        WHERE toUserID = @UserID;
    END
    ELSE
        SET @Count = 0;

    RETURN @Count;
END;
GO

-- GET SENDER NAME
CREATE FUNCTION dbo.GetSenderName(
    @TxType NVARCHAR(20),
    @TxID INT
)
RETURNS NVARCHAR(50)
AS
BEGIN
    DECLARE @Name NVARCHAR(50);

    IF @TxType = 'U2U'
    BEGIN
        SELECT @Name = su.fullName
        FROM UserToUserTransactions t
        JOIN Users su ON t.fromUserID = su.userID
        WHERE t.UToUTransactionID = @TxID;
    END
    ELSE IF @TxType = 'Regular'
    BEGIN
        SELECT @Name = u.fullName
        FROM RegularTransactions t
        JOIN Users u ON t.fromUserID = u.userID
        WHERE t.regularTransactionID = @TxID;
    END
    ELSE IF @TxType = 'TopUp'
    BEGIN
        SELECT @Name = s.SourceName
        FROM TopUpTransactions t
        JOIN TopUpSources s ON t.sourceID = s.sourceID
        WHERE t.topUpTransactionID = @TxID;
    END
    ELSE IF @TxType = 'VendorPay'
    BEGIN
        SET @Name = 'Admin';
    END

    RETURN @Name;
END;
GO


-- GET RECEIVER NAME
CREATE OR ALTER FUNCTION dbo.GetReceiverName(
    @TxType NVARCHAR(20),
    @TxID INT
)
RETURNS NVARCHAR(50)
AS
BEGIN
    DECLARE @Name NVARCHAR (50);

    IF @TxType = 'U2U'
    BEGIN
        SELECT @Name = ru.fullName
        FROM UserToUserTransactions t
        JOIN Users ru ON t.toUserID = ru.userID
        WHERE t.UToUTransactionID = @TxID;
    END
    ELSE IF @TxType = 'Regular'
    BEGIN
        SELECT @Name = v.vendorName
        FROM RegularTransactions t
        JOIN Vendors v ON t.toVendorID = v.vendorID
        WHERE t.regularTransactionID = @TxID;
    END
    ELSE IF @TxType = 'TopUp'
    BEGIN
        SELECT @Name = u.fullName
        FROM TopUpTransactions t
        JOIN Users u ON t.toUserID = u.userID
        WHERE t.topUpTransactionID = @TxID;
    END
    ELSE IF @TxType = 'VendorPay'
    BEGIN
        SELECT @Name = v.vendorName
        FROM VendorPaymentTransaction t
        JOIN Vendors v ON t.toVendorID = v.vendorID
        WHERE t.vendorPaymentTransactionID = @TxID;
    END

    RETURN @Name;
END;
GO

-- GET TRANSACTION SIGN
CREATE OR ALTER FUNCTION dbo.GetTXSign(
    @TxType NVARCHAR(20),
    @TxID INT,
    @EntityID INT,
    @EntityType NVARCHAR(10)
)
RETURNS CHAR(1)
AS
BEGIN
    DECLARE @Sign CHAR(1);

    IF @TxType = 'U2U' AND @EntityType = 'User'
    BEGIN
        DECLARE @FromUserID INT;
        SELECT @FromUserID = fromUserID 
        FROM UserToUserTransactions
        WHERE UToUTransactionID = @TxID
        SET @Sign = CASE WHEN @FromUserID = @EntityID
                    THEN '-'
                    ELSE '+'
        END;
    END
    ELSE IF @TxType = 'Regular'
    BEGIN
        IF @EntityType = 'User'
            SET @Sign = '-';
        ELSE IF @EntityType = 'Vendor'
            SET @Sign = '+';
    END
    ELSE IF @TxType = 'TopUp'
    BEGIN
        IF @EntityType = 'User'
            SET @Sign = '+';
        ELSE
            SET @Sign = NULL;
    END
    ELSE IF @TxType = 'VendorPay'
    BEGIN
        IF @EntityType = 'Vendor'
            SET @Sign = '+';
        ELSE
            SET @Sign = NULL;
    END
    ELSE
        SET @Sign = NULL;

    RETURN @Sign;
END;
GO
