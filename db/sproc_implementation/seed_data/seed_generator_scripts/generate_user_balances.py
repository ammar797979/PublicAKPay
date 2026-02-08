import random
from datetime import datetime, timedelta

# Configuration
NUM_USERS = 5000  # scaled to 5000 for 1M transaction target

def generate_balance():
    """Generate random balance between 0.01 and 50000.00"""
    # Different balance tiers for variety
    tier = random.random()
    
    if tier < 0.3:  # 30% low balance (0.01 - 500)
        balance = round(random.uniform(0.01, 500.00), 2)
    elif tier < 0.6:  # 30% medium balance (500 - 5000)
        balance = round(random.uniform(500.00, 5000.00), 2)
    elif tier < 0.85:  # 25% high balance (5000 - 20000)
        balance = round(random.uniform(5000.00, 20000.00), 2)
    else:  # 15% very high balance (20000 - 50000)
        balance = round(random.uniform(20000.00, 50000.00), 2)
    
    return balance

def generate_random_datetime():
    """Generate random datetime within last 6 months"""
    days_ago = random.randint(0, 180)  # 0 to 6 months
    random_date = datetime.now() - timedelta(days=days_ago)
    random_date = random_date.replace(
        hour=random.randint(0, 23),
        minute=random.randint(0, 59),
        second=random.randint(0, 59),
        microsecond=0
    )
    return random_date.strftime('%Y-%m-%d %H:%M:%S')

# Generate SQL
print("-- Generated INSERT statements for UserAccounts table")
print(f"-- Total records: {NUM_USERS}")
print(f"-- 95% of accounts ({int(NUM_USERS*0.95)}) set to IsActive = 1")
print("-- Generated on:", datetime.now().strftime('%Y-%m-%d %H:%M:%S'))
print()
print("USE AKPayDB;")
print("GO")
print()

for user_id in range(1, NUM_USERS + 1):
    balance = generate_balance()
    last_update = generate_random_datetime()
    active_cutoff = int(NUM_USERS * 0.95)
    if user_id <= active_cutoff:
        print(f"INSERT INTO UserAccounts (userID, userBalance, lastUpdateTime, IsActive) VALUES ({user_id}, {balance}, '{last_update}', 1);")
    else:
        print(f"INSERT INTO UserAccounts (userID, userBalance, lastUpdateTime, IsActive) VALUES ({user_id}, {balance}, '{last_update}', 0);")

print()
print("GO")
print()
print(f"-- Successfully generated {NUM_USERS} INSERT statements")
print("-- All balances are positive (> 0) and within DECIMAL(12,2) constraints")
print(f"-- 95% of accounts ({int(NUM_USERS*0.95)}) set to IsActive = 1")
print("-- lastUpdateTime randomly distributed over last 6 months")
import random
from datetime import datetime

# Configuration
NUM_USERS = 2000  # scaled from 500

def generate_balance():
    """Generate random balance between 0.01 and 50000.00"""
    # Different balance tiers for variety
    tier = random.random()
    
    if tier < 0.3:  # 30% low balance (0.01 - 500)
        balance = round(random.uniform(0.01, 500.00), 2)
    elif tier < 0.6:  # 30% medium balance (500 - 5000)
        balance = round(random.uniform(500.00, 5000.00), 2)
    elif tier < 0.85:  # 25% high balance (5000 - 20000)
        balance = round(random.uniform(5000.00, 20000.00), 2)
    else:  # 15% very high balance (20000 - 50000)
        balance = round(random.uniform(20000.00, 50000.00), 2)
    
    return balance

# Generate SQL
print("-- Generated UPDATE statements for UserAccounts table")
print(f"-- Total records: {NUM_USERS}")
print(f"-- 95% of accounts ({int(NUM_USERS*0.95)}) set to IsActive = 1")
print("-- Generated on:", datetime.now().strftime('%Y-%m-%d %H:%M:%S'))
print()
print("USE AKPayDB;")
print("GO")
print()

for user_id in range(1, NUM_USERS + 1):
    balance = generate_balance()
    active_cutoff = int(NUM_USERS * 0.95)
    if user_id <= active_cutoff:
        print(f"UPDATE UserAccounts SET userBalance = {balance}, IsActive = 1 WHERE userID = {user_id};")
    else:
        print(f"UPDATE UserAccounts SET userBalance = {balance} WHERE userID = {user_id};")

print()
print("GO")
print()
print("SELECT * FROM UserAccounts")
print("GO")
print(f"-- Successfully generated {NUM_USERS} UPDATE statements")
print("-- All balances are positive (> 0) and within DECIMAL(12,2) constraints")
print(f"-- 95% of accounts ({int(NUM_USERS*0.95)}) set to IsActive = 1")
