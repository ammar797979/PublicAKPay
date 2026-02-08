import random
from datetime import datetime, timedelta

# Configuration
NUM_TRANSACTIONS = 100000  # Generate 100K user-to-user transactions for 1M target
NUM_USERS = 5000           # User IDs range from 1..5000
BATCH_SIZE = 1000          # Insert 1000 rows at a time

# Transaction status IDs mapping (from seed data):
# 1 = Pending, 2 = Sync Pending, 3 = Invalid, 4 = Sync Failed, 5 = Accepted

def generate_status_id():
    """Generate status ID with a distribution (U2U slightly less reliable than top-up)."""
    r = random.random()
    if r < 0.60:       # 60% accepted
        return 5
    elif r < 0.72:     # 12% pending
        return 1
    elif r < 0.84:     # 12% sync pending
        return 2
    elif r < 0.93:     # 9% sync failed
        return 4
    else:              # 7% invalid
        return 3

def generate_amount():
    """Generate realistic peer transfer amounts."""
    r = random.random()
    if r < 0.50:          # 50% micro transfers 10 - 300
        return round(random.uniform(10.00, 300.00), 2)
    elif r < 0.80:        # 30% small 300 - 1500
        return round(random.uniform(300.00, 1500.00), 2)
    elif r < 0.95:        # 15% medium 1500 - 5000
        return round(random.uniform(1500.00, 5000.00), 2)
    else:                 # 5% large 5000 - 12000
        return round(random.uniform(5000.00, 12000.00), 2)

def generate_random_datetime():
    """Generate random datetime within last 4 months (more recent activity)."""
    days_ago = random.randint(0, 120)
    dt = datetime.now() - timedelta(days=days_ago)
    hour = random.choices(range(24), weights=[1,1,1,1,1,2,3,4,5,5,5,5,5,5,5,5,5,5,4,4,3,2,1,1], k=1)[0]
    dt = dt.replace(hour=hour, minute=random.randint(0,59), second=random.randint(0,59), microsecond=0)
    return dt.strftime('%Y-%m-%d %H:%M:%S')

# Generate SQL header
print("-- Generated BATCHED INSERT statements for UserToUserTransactions table")
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

# Generate all transactions first
transactions_data = []
for _ in range(NUM_TRANSACTIONS):
    from_user, to_user = random.sample(range(1, NUM_USERS + 1), 2)
    amount = generate_amount()
    ts = generate_random_datetime()
    status_id = generate_status_id()
    
    transactions_data.append({
        'to_user': to_user,
        'from_user': from_user,
        'amount': amount,
        'ts': ts,
        'status_id': status_id
    })

# Insert in batches
for batch_start in range(0, len(transactions_data), BATCH_SIZE):
    batch = transactions_data[batch_start:batch_start + BATCH_SIZE]
    
    print("INSERT INTO UserToUserTransactions (toUserID, fromUserID, amount, txTimeStamp, txStatusID)")
    print("VALUES")
    
    for idx, tx in enumerate(batch):
        comma = "," if idx < len(batch) - 1 else ";"
        print(f"    ({tx['to_user']}, {tx['from_user']}, {tx['amount']}, '{tx['ts']}', {tx['status_id']}){comma}")
    
    print()
    print("GO")
    print()

print(f"-- Successfully generated {NUM_TRANSACTIONS} INSERT statements in batches of {BATCH_SIZE}")
print("-- Status distribution approx: 60% Accepted, 12% Pending, 12% Sync Pending, 9% Sync Failed, 7% Invalid")
print("-- Amount tiers: 50% 10-300, 30% 300-1500, 15% 1500-5000, 5% 5000-12000")
print("-- Timestamp window: Last 4 months")
print("-- All records enforce toUserID <> fromUserID and amount > 0")
