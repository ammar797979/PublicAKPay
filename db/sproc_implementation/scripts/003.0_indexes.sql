USE AKPayDB;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Notifications_RecipientID' AND object_id = OBJECT_ID(N'dbo.Notifications')
)
BEGIN
    CREATE INDEX IX_Notifications_RecipientID
        ON Notifications(recipientID);
END;
GO
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Users_isDeleted' AND object_id = OBJECT_ID(N'dbo.Users')
)
BEGIN
    CREATE INDEX IX_Users_isDeleted
        ON Users(isDeleted);
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Users_Email_Active' AND object_id = OBJECT_ID(N'dbo.Users')
)  
BEGIN
    CREATE UNIQUE INDEX IX_Users_Email_Active
        ON Users(email)
        WHERE Users.isDeleted = 0;
END;
GO
-- NOT REALLY NEEDED AS PHONE NOT SEARCHED ON ALOT

-- IF NOT EXISTS (
--     SELECT 1 FROM sys.indexes 
--     WHERE name = 'IX_Users_Phone_Active' AND object_id = OBJECT_ID(N'dbo.Users')
-- )
-- BEGIN
--     CREATE UNIQUE INDEX IX_Users_Phone_Active
--         ON Users(phone)
--         WHERE Users.isDeleted = 0;
-- END;
-- GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Users_UserName_Active' AND object_id = OBJECT_ID(N'dbo.Users')
)
BEGIN
    CREATE UNIQUE INDEX IX_Users_UserName_Active
        ON Users(userName)
        WHERE Users.isDeleted = 0;
END;
GO

-- RegularTransactions FKs
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_RegTx_FromUserID' AND object_id = OBJECT_ID(N'dbo.RegularTransactions')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_RegTx_FromUserID ON RegularTransactions(fromUserID);
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_RegTx_ToVendorID' AND object_id = OBJECT_ID(N'dbo.RegularTransactions')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_RegTx_ToVendorID 
        ON RegularTransactions(toVendorID);
END;
GO

-- TopUpTransactions FK
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_TopUpTx_ToUserID' AND object_id = OBJECT_ID(N'dbo.TopUpTransactions')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_TopUpTx_ToUserID 
        ON TopUpTransactions(toUserID);
END;
GO

-- UserToUserTransactions FKs
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_U2UTx_FromUserID' AND object_id = OBJECT_ID(N'dbo.UserToUserTransactions')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_U2UTx_FromUserID
        ON UserToUserTransactions(fromUserID);
END;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_U2UTx_ToUserID' AND object_id = OBJECT_ID(N'dbo.UserToUserTransactions')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_U2UTx_ToUserID 
        ON UserToUserTransactions(toUserID);
END;
GO

-- VendorPaymentTransaction FK (table name is singular)
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_VendorPayTx_ToVendorID' AND object_id = OBJECT_ID(N'dbo.VendorPaymentTransaction')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_VendorPayTx_ToVendorID 
        ON VendorPaymentTransaction(toVendorID);
END;
GO


-- RegularTransactions
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_RegTx_Status_Time' AND object_id = OBJECT_ID(N'dbo.RegularTransactions')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_RegTx_Status_Time
        ON RegularTransactions(txStatusID, txTimeStamp)
        INCLUDE (amount, fromUserID);
END;
GO

-- TopUpTransactions
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_TopUpTx_Status_Time' AND object_id = OBJECT_ID(N'dbo.TopUpTransactions')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_TopUpTx_Status_Time
        ON TopUpTransactions(txStatusID, txTimeStamp)
        INCLUDE (amount);
END;
GO

-- UserToUserTransactions
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_U2UTx_Status_Time' AND object_id = OBJECT_ID(N'dbo.UserToUserTransactions')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_U2UTx_Status_Time
        ON UserToUserTransactions(txStatusID, txTimeStamp)
        INCLUDE (amount);
END;
GO

-- VendorPaymentTransaction
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_VendorPayTx_Status_Time' AND object_id = OBJECT_ID(N'dbo.VendorPaymentTransaction')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_VendorPayTx_Status_Time
        ON VendorPaymentTransaction(txStatusID, txTimeStamp)
        INCLUDE (amount);
END;
GO
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Notifications_RecipientID' AND object_id = OBJECT_ID(N'dbo.Notifications')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Notifications_RecipientID
        ON Notifications(recipientID)
        ON ps_TransactionDate(createdAt); -- Aligns index with the data partition
END;
GO