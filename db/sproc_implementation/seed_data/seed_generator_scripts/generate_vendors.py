import random
from datetime import datetime, timedelta

# Configuration
NUM_VENDORS = 0  # All 17 vendors created in seed_data/002_adding_values.sql - no additional vendors needed
NULL_MANAGER_NAME_PERCENTAGE = 0.03  # 3%
NULL_MANAGER_PHONE_PERCENTAGE = 0.03  # 3%
ON_BREAK_PERCENTAGE = 0.10  # 10%

# Vendor names (updated list requested)
restaurant_names = [
    'LUMS Pharmacy',
    'LUMS Store',
    'Zakir',
    'Sweet Moments',
    'PDC',
    'Baithak',
    'Off-side',
    'KIMS',
    'Subway',
    'Chop chop',
    'Juice Zone',
    'Mastani',
    'Jammin Java',
    'Delish',
    'Sweet Moments',
    'Kaashi',
    'Printing Quota',
    'Bunker',
    'Green Olive'
]
# Extend vendor names list dynamically if needed
if NUM_VENDORS > len(restaurant_names):
    base_len = len(restaurant_names)
    for i in range(base_len + 1, NUM_VENDORS + 1):
        restaurant_names.append(f'Generated Vendor {i}')

# Manager names
manager_first_names = [
    "Ali", "Ahmed", "Muhammad", "Hassan", "Usman", "Bilal", "Hamza", "Zain", "Omar", "Fahad",
    "Ayesha", "Fatima", "Zainab", "Maryam", "Aisha", "Sara", "Hira", "Noor", "Amna", "Iqra",
    "Abdullah", "Ibrahim", "Yusuf", "Imran", "Adnan", "Arslan", "Kamran", "Tariq", "Saad", "Asad"
]

manager_last_names = [
    "Khan", "Ali", "Ahmed", "Hassan", "Hussain", "Shah", "Malik", "Iqbal", "Raza", "Siddiqui",
    "Butt", "Chaudhry", "Sheikh", "Mahmood", "Akhtar", "Aziz", "Haider", "Javed", "Mirza", "Naqvi"
]

# Vendor status IDs:
# 1 = Open, 2 = Closed, 3 = On Break, 4 = Permanently Closed, 5 = Suspended
def generate_status_id():
    """Generate status ID with proper distribution"""
    rand = random.random()
    
    if rand < ON_BREAK_PERCENTAGE:  # 10% on break
        return 3
    elif rand < ON_BREAK_PERCENTAGE + 0.05:  # 5% permanently closed or suspended
        return random.choice([4, 5])
    else:  # Remaining 85% are either Open (1) or Closed (2)
        return random.choice([1, 2])

def generate_balance():
    """Generate random vendor balance between 0 and 100000.00"""
    tier = random.random()
    
    if tier < 0.3:  # 30% low balance (0 - 5000)
        balance = round(random.uniform(0.00, 5000.00), 2)
    elif tier < 0.6:  # 30% medium balance (5000 - 30000)
        balance = round(random.uniform(5000.00, 30000.00), 2)
    else:  # 40% high balance (30000 - 100000)
        balance = round(random.uniform(30000.00, 100000.00), 2)
    
    return balance

def generate_phone_number(existing_phones):
    """Generate unique 10-digit phone starting with 3"""
    while True:
        phone = "3" + "".join([str(random.randint(0, 9)) for _ in range(9)])
        if phone not in existing_phones:
            existing_phones.add(phone)
            return phone

def generate_manager_name():
    """Generate random manager name"""
    first = random.choice(manager_first_names)
    last = random.choice(manager_last_names)
    return f"{first} {last}"

def generate_random_date():
    """Generate random datetime within last year"""
    days_ago = random.randint(0, 365)
    random_date = datetime.now() - timedelta(days=days_ago)
    random_date = random_date.replace(
        hour=random.randint(0, 23),
        minute=random.randint(0, 59),
        second=random.randint(0, 59),
        microsecond=0
    )
    return random_date.strftime('%Y-%m-%d %H:%M:%S')

# Track used phones
used_phones = set()

# Calculate how many should have NULL manager name (3%)
null_manager_name_count = int(NUM_VENDORS * NULL_MANAGER_NAME_PERCENTAGE)
if null_manager_name_count < 1:
    null_manager_name_count = 1

# Calculate how many should have NULL manager phone (3%, but different from null names)
null_manager_phone_count = int(NUM_VENDORS * NULL_MANAGER_PHONE_PERCENTAGE)
if null_manager_phone_count < 1:
    null_manager_phone_count = 1

# Select random indices for NULL values (ensuring they don't overlap)
null_name_indices = set(random.sample(range(NUM_VENDORS), null_manager_name_count))
available_for_null_phone = [i for i in range(NUM_VENDORS) if i not in null_name_indices]
null_phone_indices = set(random.sample(available_for_null_phone, min(null_manager_phone_count, len(available_for_null_phone))))

# Generate SQL
print("-- Generated INSERT statements for Vendors table")
print(f"-- Total records: {NUM_VENDORS}")
print("-- Generated on:", datetime.now().strftime('%Y-%m-%d %H:%M:%S'))
print()
print("USE AKPayDB;")
print("GO")
print()

for i in range(NUM_VENDORS):
    vendor_name = restaurant_names[i]
    vendor_balance = generate_balance()
    last_update_time = generate_random_date()
    status_id = generate_status_id()
    
    # Determine manager name (NULL for ~3%)
    if i in null_name_indices:
        manager_name = "NULL"
        manager_phone = "NULL"  # If no manager name, no phone either
    else:
        manager_name = f"'{generate_manager_name()}'"
        
        # Determine manager phone (NULL for ~3% of non-null names)
        if i in null_phone_indices:
            manager_phone = "NULL"
        else:
            manager_phone = f"'{generate_phone_number(used_phones)}'"
    
    # Escape single quotes in vendor name for SQL
    vendor_name_escaped = vendor_name.replace("'", "''")
    
    # Generate INSERT statement
    # Note: vendorID is IDENTITY so we don't insert it
    print(f"INSERT INTO Vendors (vendorName, vendorBalance, lastUpdateTime, managerName, managerPhone, statusID)")
    print(f"VALUES ('{vendor_name_escaped}', {vendor_balance}, '{last_update_time}', {manager_name}, {manager_phone}, {status_id});")

print()
print("GO")
print()
print(f"-- Successfully generated {NUM_VENDORS} INSERT statements")
print(f"-- Approximately {null_manager_name_count} records have NULL managerName ({NULL_MANAGER_NAME_PERCENTAGE*100}%)")
print(f"-- Approximately {null_manager_phone_count} records have NULL managerPhone ({NULL_MANAGER_PHONE_PERCENTAGE*100}%)")
print(f"-- Approximately {int(NUM_VENDORS * ON_BREAK_PERCENTAGE)} records have status 'On Break' ({ON_BREAK_PERCENTAGE*100}%)")
print(f"-- Majority have status 'Open' (1) or 'Closed' (2)")
print(f"-- Few have status 'Permanently Closed' (4) or 'Suspended' (5)")
import random
from datetime import datetime, timedelta

# Configuration
NUM_VENDORS = 0  # All 17 vendors created in seed_data/002_adding_values.sql - no additional vendors needed
NULL_MANAGER_NAME_PERCENTAGE = 0.03  # 3%
NULL_MANAGER_PHONE_PERCENTAGE = 0.03  # 3%
ON_BREAK_PERCENTAGE = 0.10  # 10%

# Vendor names (updated list requested)
restaurant_names = [
    'LUMS Pharmacy',
    'LUMS Store',
    'Zakir',
    'Sweet Moments',
    'PDC',
    'Baithak',
    'Off-side',
    'KIMS',
    'Subway',
    'Chop chop',
    'Juice Zone',
    'Mastani',
    'Jammin Java',
    'Delish',
    'Sweet Moments',
    'Kaashi',
    'Printing Quota',
    'Bunker',
    'Green Olive'
]
# Extend vendor names list dynamically if needed
if NUM_VENDORS > len(restaurant_names):
    base_len = len(restaurant_names)
    for i in range(base_len + 1, NUM_VENDORS + 1):
        restaurant_names.append(f'Generated Vendor {i}')

# Manager names
manager_first_names = [
    "Ali", "Ahmed", "Muhammad", "Hassan", "Usman", "Bilal", "Hamza", "Zain", "Omar", "Fahad",
    "Ayesha", "Fatima", "Zainab", "Maryam", "Aisha", "Sara", "Hira", "Noor", "Amna", "Iqra",
    "Abdullah", "Ibrahim", "Yusuf", "Imran", "Adnan", "Arslan", "Kamran", "Tariq", "Saad", "Asad"
]

manager_last_names = [
    "Khan", "Ali", "Ahmed", "Hassan", "Hussain", "Shah", "Malik", "Iqbal", "Raza", "Siddiqui",
    "Butt", "Chaudhry", "Sheikh", "Mahmood", "Akhtar", "Aziz", "Haider", "Javed", "Mirza", "Naqvi"
]

# Vendor status IDs:
# 1 = Open, 2 = Closed, 3 = On Break, 4 = Permanently Closed, 5 = Suspended
def generate_status_id():
    """Generate status ID with proper distribution"""
    rand = random.random()
    
    if rand < ON_BREAK_PERCENTAGE:  # 10% on break
        return 3
    elif rand < ON_BREAK_PERCENTAGE + 0.05:  # 5% permanently closed or suspended
        return random.choice([4, 5])
    else:  # Remaining 85% are either Open (1) or Closed (2)
        return random.choice([1, 2])

def generate_balance():
    """Generate random vendor balance between 0 and 100000.00"""
    tier = random.random()
    
    if tier < 0.3:  # 30% low balance (0 - 5000)
        balance = round(random.uniform(0.00, 5000.00), 2)
    elif tier < 0.6:  # 30% medium balance (5000 - 30000)
        balance = round(random.uniform(5000.00, 30000.00), 2)
    else:  # 40% high balance (30000 - 100000)
        balance = round(random.uniform(30000.00, 100000.00), 2)
    
    return balance

def generate_phone_number(existing_phones):
    """Generate unique 11-digit phone starting with 3"""
    while True:
        phone = "3" + "".join([str(random.randint(0, 9)) for _ in range(10)])
        if phone not in existing_phones:
            existing_phones.add(phone)
            return phone

def generate_manager_name():
    """Generate random manager name"""
    first = random.choice(manager_first_names)
    last = random.choice(manager_last_names)
    return f"{first} {last}"

def generate_random_date():
    """Generate random datetime within last year"""
    days_ago = random.randint(0, 365)
    random_date = datetime.now() - timedelta(days=days_ago)
    random_date = random_date.replace(
        hour=random.randint(0, 23),
        minute=random.randint(0, 59),
        second=random.randint(0, 59),
        microsecond=0
    )
    return random_date.strftime('%Y-%m-%d %H:%M:%S')

# Track used phones
used_phones = set()

# Calculate how many should have NULL manager name (3%)
null_manager_name_count = int(NUM_VENDORS * NULL_MANAGER_NAME_PERCENTAGE)
if null_manager_name_count < 1:
    null_manager_name_count = 1

# Calculate how many should have NULL manager phone (3%, but different from null names)
null_manager_phone_count = int(NUM_VENDORS * NULL_MANAGER_PHONE_PERCENTAGE)
if null_manager_phone_count < 1:
    null_manager_phone_count = 1

# Select random indices for NULL values (ensuring they don't overlap)
null_name_indices = set(random.sample(range(NUM_VENDORS), null_manager_name_count))
available_for_null_phone = [i for i in range(NUM_VENDORS) if i not in null_name_indices]
null_phone_indices = set(random.sample(available_for_null_phone, min(null_manager_phone_count, len(available_for_null_phone))))

# Generate SQL
print("-- Generated INSERT statements for Vendors table")
print(f"-- Total records: {NUM_VENDORS}")
print("-- Generated on:", datetime.now().strftime('%Y-%m-%d %H:%M:%S'))
print()
print("USE AKPayDB;")
print("GO")
print()

for i in range(NUM_VENDORS):
    vendor_name = restaurant_names[i]
    vendor_balance = generate_balance()
    last_update_time = generate_random_date()
    status_id = generate_status_id()
    
    # Determine manager name (NULL for ~3%)
    if i in null_name_indices:
        manager_name = "NULL"
        manager_phone = "NULL"  # If no manager name, no phone either
    else:
        manager_name = f"'{generate_manager_name()}'"
        
        # Determine manager phone (NULL for ~3% of non-null names)
        if i in null_phone_indices:
            manager_phone = "NULL"
        else:
            manager_phone = f"'{generate_phone_number(used_phones)}'"
    
    # Escape single quotes in vendor name for SQL
    vendor_name_escaped = vendor_name.replace("'", "''")
    
    # Generate INSERT statement
    # Note: vendorID is IDENTITY so we don't insert it
    print(f"INSERT INTO Vendors (vendorName, vendorBalance, lastUpdateTime, managerName, managerPhone, statusID)")
    print(f"VALUES ('{vendor_name_escaped}', {vendor_balance}, '{last_update_time}', {manager_name}, {manager_phone}, {status_id});")

print()
print("GO")
print()
print(f"-- Successfully generated {NUM_VENDORS} INSERT statements")
print(f"-- Approximately {null_manager_name_count} records have NULL managerName ({NULL_MANAGER_NAME_PERCENTAGE*100}%)")
print(f"-- Approximately {null_manager_phone_count} records have NULL managerPhone ({NULL_MANAGER_PHONE_PERCENTAGE*100}%)")
print(f"-- Approximately {int(NUM_VENDORS * ON_BREAK_PERCENTAGE)} records have status 'On Break' ({ON_BREAK_PERCENTAGE*100}%)")
print(f"-- Majority have status 'Open' (1) or 'Closed' (2)")
print(f"-- Few have status 'Permanently Closed' (4) or 'Suspended' (5)")
