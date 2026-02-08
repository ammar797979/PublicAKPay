import random
from datetime import datetime, timedelta

# Configuration
NUM_TRANSACTIONS = 100000  # scaled to 100K for 1M+ transaction target
BATCH_SIZE = 1000  # Insert 1000 rows at a time

# Transaction status IDs:
# 1 = Pending, 2 = Sync Pending, 3 = Invalid, 4 = Sync Failed, 5 = Accepted
def generate_status_id():
    """Generate status ID with realistic distribution for vendor payments"""
    rand = random.random()
    
    # These are admin payments, so very high success rate
    if rand < 0.85:  # 85% accepted
        return 5
    elif rand < 0.93:  # 8% sync pending
        return 2
    elif rand < 0.98:  # 5% sync failed
        return 4
    else:  # 2% pending
        return 1

def generate_amount():
    """Generate realistic vendor payment amount"""
    tier = random.random()
    
    # Vendor payments are usually larger amounts (settlement payments)
    if tier < 0.20:  # 20% small settlements (1000 - 10000)
        amount = round(random.uniform(1000.00, 10000.00), 2)
    elif tier < 0.50:  # 30% medium settlements (10000 - 50000)
        amount = round(random.uniform(10000.00, 50000.00), 2)
    elif tier < 0.80:  # 30% large settlements (50000 - 150000)
        amount = round(random.uniform(50000.00, 150000.00), 2)
    else:  # 20% very large settlements (150000 - 500000)
        amount = round(random.uniform(150000.00, 500000.00), 2)
    
    return amount

def generate_random_datetime():
    """Generate random datetime within last year"""
    days_ago = random.randint(0, 365)  # 0 to 1 year
    random_date = datetime.now() - timedelta(days=days_ago)
    
    # Admin payments typically happen during business hours (9 AM - 5 PM)
    hour = random.choices(
        range(24),
        weights=[0, 0, 0, 0, 0, 0, 0, 0, 1, 4, 5, 5, 5, 4, 5, 5, 4, 2, 0, 0, 0, 0, 0, 0],
        k=1
    )[0]
    
    random_date = random_date.replace(
        hour=hour,
        minute=random.randint(0, 59),
        second=random.randint(0, 59),
        microsecond=0
    )
    return random_date.strftime('%Y-%m-%d %H:%M:%S')

# Generate SQL
print("-- Generated BATCHED INSERT statements for VendorPaymentTransaction table")
print(f"-- Total records: {NUM_TRANSACTIONS}")
print(f"-- Batch size: {BATCH_SIZE}")
print("-- Generated on:", datetime.now().strftime('%Y-%m-%d %H:%M:%S'))
print()
print("USE AKPayDB;")
print("GO")
print()
print("SET NOCOUNT ON;")
print("GO")
print()

# We have 17 vendors
NUM_VENDORS = 17

# Generate all transactions first
transactions_data = []
# Each vendor should receive multiple payments - create weighted distribution
vendor_weights = [random.randint(3, 12) for _ in range(NUM_VENDORS)]

for i in range(NUM_TRANSACTIONS):
    to_vendor_id = random.choices(range(1, NUM_VENDORS + 1), weights=vendor_weights, k=1)[0]
    amount = generate_amount()
    tx_timestamp = generate_random_datetime()
    tx_status_id = generate_status_id()
    
    transactions_data.append({
        'to_vendor_id': to_vendor_id,
        'amount': amount,
        'tx_timestamp': tx_timestamp,
        'tx_status_id': tx_status_id
    })

# Insert in batches
for batch_start in range(0, len(transactions_data), BATCH_SIZE):
    batch = transactions_data[batch_start:batch_start + BATCH_SIZE]
    
    print("INSERT INTO VendorPaymentTransaction (toVendorID, amount, txTimeStamp, txStatusID)")
    print("VALUES")
    
    for idx, tx in enumerate(batch):
        comma = "," if idx < len(batch) - 1 else ";"
        print(f"    ({tx['to_vendor_id']}, {tx['amount']}, '{tx['tx_timestamp']}', {tx['tx_status_id']}){comma}")
    
    print()
    print("GO")
    print()

print(f"-- Successfully generated {NUM_TRANSACTIONS} INSERT statements in batches of {BATCH_SIZE}")
print("-- Status distribution: ~85% Accepted, ~8% Sync Pending, ~5% Sync Failed, ~2% Pending")
print("-- Amount range: 1000.00 to 500000.00 (settlement amounts)")
print("-- Timestamp range: Last 1 year (business hours)")
print("-- All payments are from admin to vendors")
