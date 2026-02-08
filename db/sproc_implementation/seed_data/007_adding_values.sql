USE AKPayDB;
GO

-- Populate VendorStatuses Table
INSERT INTO VendorStatuses (statusName)
VALUES
    ('Open'),
    ('Closed'),
    ('On Break'),
    ('Permanently Closed'),
    ('Suspended');
GO

-- Populate TransactionStatuses Table
INSERT INTO TransactionStatuses (statusName, statusDescription)
VALUES 
    ('Pending', 'Pending sync AND validation'),
    ('Sync Pending', 'Validated BUT pending sync'),
    ('Invalid', 'Validation failed, NO need to attempt sync'),
    ('Sync Failed', 'Valid BUT sync failed'),
    ('Accepted', 'Valid AND synced');
GO

-- Populate TopUpSources Table
INSERT INTO TopUpSources (sourceName)
VALUES  ('Admin'),
        ('Bank'),
        ('Card'),
        ('Cash'),
        ('Mobile Wallet'),
        ('Cheque');
GO