CREATE OR ALTER PROCEDURE sp_GetDailySpendingTrends
    @TargetMonth INT,
    @TargetYear INT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Construct the Date Range (Start of Month to Start of Next Month)
    -- This keeps the query SARGable so it uses your Indexes & Partitions.
    DECLARE @StartDate DATE = DATEFROMPARTS(@TargetYear, @TargetMonth, 1);
    DECLARE @EndDate DATE = DATEADD(MONTH, 1, @StartDate);

    SELECT 
        DAY(txTimeStamp) AS DayOfMonth,
        COUNT(regularTransactionID) as TransactionVolume,
        SUM(amount) as TotalSpent,
        AVG(amount) as AvgTicketSize
    FROM RegularTransactions
    WHERE txTimeStamp >= @StartDate 
      AND txTimeStamp < @EndDate -- Range Query (SARGable)
      AND txStatusID = 5 -- Accepted
    GROUP BY DAY(txTimeStamp)
    ORDER BY DAY(txTimeStamp);
END;
GO