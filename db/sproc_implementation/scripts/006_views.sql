-- Chcecks current user balance
CREATE OR ALTER VIEW dbo.CheckBalance AS
SELECT
    u.userID,
    u.fullName AS username,
    ua.userBalance AS balance,
    ua.lastUpdateTime
FROM Users u
JOIN UserAccounts ua
    ON ua.UserID = u.UserID
GO

-- Chcecks Vendor Status
CREATE OR ALTER VIEW dbo.VendorStatus AS
SELECT
    v.vendorID,
    v.vendorName,
    v.vendorBalance,
    vs.statusName AS CurrentStatus,
    v.managerName,
    v.managerPhone,
    v.lastUpdateTime
FROM Vendors v
JOIN VendorStatuses vs
    ON v.statusID = vs.statusID;
GO
