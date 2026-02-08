
-- Covering index for time-based analytics (daily/monthly trends)
-- Includes amount, txStatusID, fromUserID needed by analytic queries to reduce key lookups
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RegularTransactions_Analytics')
BEGIN
    CREATE NONCLUSTERED INDEX IX_RegularTransactions_Analytics
        ON RegularTransactions (txTimeStamp)
        INCLUDE (amount, txStatusID, fromUserID);
END;
GO

-- CTEs
-- Vendor performance rankings (most revenue first)
CREATE OR ALTER VIEW vw_VendorPerformanceRankings AS
WITH VendorRankings AS (
    SELECT 
        v.vendorName,
        COUNT(rt.regularTransactionID) AS TotalTransactions,
        SUM(rt.amount) AS TotalRevenue,
        RANK() OVER (ORDER BY SUM(rt.amount) DESC) AS RankPosition
    FROM Vendors v
    JOIN RegularTransactions rt ON v.vendorID = rt.toVendorID
    JOIN TransactionStatuses ts ON rt.txStatusID = ts.statusID
    WHERE ts.statusName = 'Accepted'
    GROUP BY v.vendorName
)
SELECT * FROM VendorRankings;
GO

-- Monthly user spending statistics (dashboard pre-aggregation)
CREATE OR ALTER VIEW vw_UserMonthlyStats AS
WITH MonthlyStats AS (
    SELECT 
        fromUserID,
        FORMAT(txTimeStamp, 'yyyy-MM') AS TransactionMonth,
        AVG(amount) AS AvgTransactionSize,
        SUM(amount) AS TotalSpent
    FROM RegularTransactions rt
    JOIN TransactionStatuses ts ON rt.txStatusID = ts.statusID
    WHERE ts.statusName = 'Accepted'
    GROUP BY fromUserID, FORMAT(txTimeStamp, 'yyyy-MM')
)
SELECT 
    u.fullName AS userFullName,
    u.email,
    ms.TransactionMonth,
    ms.TotalSpent,
    ms.AvgTransactionSize
FROM MonthlyStats ms
JOIN Users u ON ms.fromUserID = u.userID;
GO
