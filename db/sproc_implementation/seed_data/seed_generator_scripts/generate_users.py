import random
import hashlib
from datetime import datetime, timedelta

# Configuration
NUM_USERS = 5000  # scaled to 5000 for 1M transaction target
NULL_USERTYPE_PERCENTAGE = 0.10
DELETED_PERCENTAGE = 0.05  # 5% of users will be marked as deleted
BATCH_SIZE = 1000  # Insert 1000 rows at a time

# Generate random names
first_names = [
    "Ali", "Ahmed", "Muhammad", "Hassan", "Usman", "Bilal", "Hamza", "Zain", "Omar", "Fahad",
    "Ayesha", "Fatima", "Zainab", "Maryam", "Aisha", "Sara", "Hira", "Noor", "Amna", "Iqra",
    "Abdullah", "Ibrahim", "Yusuf", "Imran", "Adnan", "Arslan", "Kamran", "Tariq", "Saad", "Asad",
    "Sana", "Mahnoor", "Rabia", "Khadija", "Alina", "Maria", "Sidra", "Nimra", "Farhan", "Raza",
    "Talha", "Junaid", "Shahzad", "Waqas", "Naveed", "Farooq", "Kashif", "Rizwan", "Faisal", "Sami",
    "Bushra", "Shamsa", "Tuba", "Uzma", "Samina", "Nazia", "Rubina", "Shazia", "Farzana", "Naseem",
    "Asim", "Zahid", "Khalid", "Rashid", "Majid", "Salman", "Irfan", "Wasim", "Nadeem", "Hanif",
    "Sadaf", "Humaira", "Saima", "Shaista", "Fouzia", "Naila", "Shama", "Yasmin", "Parveen", "Tahira",
    "Aamir", "Saleem", "Shahid", "Arshad", "Jamil", "Akram", "Anwar", "Aslam", "Azhar", "Rafiq",
    "Nadia", "Riffat", "Sumera", "Amjad", "Aftab", "Qamar", "Shafiq", "Shakeel", "Tanvir", "Zaheer"
]

last_names = [
    "Khan", "Ali", "Ahmed", "Hassan", "Hussain", "Shah", "Malik", "Iqbal", "Raza", "Siddiqui",
    "Butt", "Chaudhry", "Sheikh", "Mahmood", "Akhtar", "Aziz", "Haider", "Javed", "Mirza", "Naqvi",
    "Qureshi", "Rasheed", "Saeed", "Tahir", "Umar", "Waseem", "Yousaf", "Zaidi", "Abbas", "Akbar",
    "Aslam", "Ayub", "Bashir", "Farooq", "Ghani", "Hameed", "Ilyas", "Jamil", "Karim", "Latif",
    "Masood", "Nadeem", "Qadir", "Rafiq", "Sadiq", "Tariq", "Waheed", "Yaqoob", "Zia", "Anwar"
]

# User types
user_types = ["student", "teacher", "worker", "admin"]

def generate_password_hash(user_id):
    """Generate a simple hash for password"""
    password = f"password{user_id}"
    return hashlib.sha256(password.encode()).hexdigest()

def generate_phone_number(existing_phones):
    """Generate unique 10-digit phone starting with 3"""
    while True:
        phone = "3" + "".join([str(random.randint(0, 9)) for _ in range(9)])
        if phone not in existing_phones:
            existing_phones.add(phone)
            return phone

def generate_email(user_id, existing_emails):
    """Generate unique email ending with @lums.edu.pk"""
    while True:
        formats = [
            f"user{user_id}@lums.edu.pk",
            f"{user_id:05d}@lums.edu.pk",
            f"student{user_id}@lums.edu.pk",
            f"lums{user_id}@lums.edu.pk"
        ]
        email = random.choice(formats)
        if email not in existing_emails:
            existing_emails.add(email)
            return email

def generate_random_date():
    """Generate random datetime within last 2 years"""
    days_ago = random.randint(0, 730)
    random_date = datetime.now() - timedelta(days=days_ago)
    random_date = random_date.replace(
        hour=random.randint(0, 23),
        minute=random.randint(0, 59),
        second=random.randint(0, 59),
        microsecond=0
    )
    return random_date.strftime('%Y-%m-%d %H:%M:%S')

def generate_deletion_date(date_created_str):
    """Generate random deletion datetime after the account was created"""
    date_created = datetime.strptime(date_created_str, '%Y-%m-%d %H:%M:%S')
    days_diff = (datetime.now() - date_created).days
    
    if days_diff > 0:
        days_after_creation = random.randint(1, max(1, days_diff))
        deletion_date = date_created + timedelta(days=days_after_creation)
        deletion_date = deletion_date.replace(
            hour=random.randint(0, 23),
            minute=random.randint(0, 59),
            second=random.randint(0, 59),
            microsecond=0
        )
        return deletion_date.strftime('%Y-%m-%d %H:%M:%S')
    else:
        deletion_date = date_created + timedelta(hours=random.randint(1, 24))
        return deletion_date.strftime('%Y-%m-%d %H:%M:%S')

def generate_full_name():
    """Generate random full name"""
    first = random.choice(first_names)
    last = random.choice(last_names)
    return f"{first} {last}"

# Track used values to ensure uniqueness
used_emails = set()
used_phones = set()

# Generate SQL header
print("-- Generated BATCHED INSERT statements for Users table")
print(f"-- Total records: {NUM_USERS}")
print(f"-- Batch size: {BATCH_SIZE}")
print(f"-- {int(NUM_USERS * DELETED_PERCENTAGE)} accounts marked as deleted ({DELETED_PERCENTAGE*100}%)")
print("-- Generated on:", datetime.now().strftime('%Y-%m-%d %H:%M:%S'))
print()
print("USE AKPayDB;")
print("GO")
print()
print("SET NOCOUNT ON;")
print("GO")
print()

# Calculate distributions
null_usertype_count = int(NUM_USERS * NULL_USERTYPE_PERCENTAGE)
null_indices = set(random.sample(range(NUM_USERS), null_usertype_count))
deleted_count = int(NUM_USERS * DELETED_PERCENTAGE)
deleted_indices = set(random.sample(range(NUM_USERS), deleted_count))

# Generate all users first
users_data = []
for i in range(NUM_USERS):
    user_id = i + 1
    email = generate_email(user_id, used_emails)
    phone = generate_phone_number(used_phones)
    full_name = generate_full_name()
    password_hash = generate_password_hash(user_id)
    
    if i in null_indices:
        user_type = "NULL"
    else:
        user_type = f"'{random.choice(user_types)}'"
    
    date_created = generate_random_date()
    
    if i in deleted_indices:
        is_deleted = 1
        date_deleted = generate_deletion_date(date_created)
        date_deleted_str = f"'{date_deleted}'"
    else:
        is_deleted = 0
        date_deleted_str = "NULL"
    
    users_data.append({
        'email': email,
        'phone': phone,
        'full_name': full_name,
        'password_hash': password_hash,
        'user_type': user_type,
        'date_created': date_created,
        'is_deleted': is_deleted,
        'date_deleted_str': date_deleted_str
    })

# Insert in batches
for batch_start in range(0, len(users_data), BATCH_SIZE):
    batch = users_data[batch_start:batch_start + BATCH_SIZE]
    
    print("INSERT INTO Users (email, phone, fullName, passwordHash, userType, dateCreated, isDeleted, deletedAt)")
    print("VALUES")
    
    for idx, user in enumerate(batch):
        comma = "," if idx < len(batch) - 1 else ";"
        print(f"    ('{user['email']}', '{user['phone']}', '{user['full_name']}', '{user['password_hash']}', {user['user_type']}, '{user['date_created']}', {user['is_deleted']}, {user['date_deleted_str']}){comma}")
    
    print()
    print("GO")
    print()

print(f"-- Successfully generated {NUM_USERS} INSERT statements in batches of {BATCH_SIZE}")
print(f"-- Approximately {null_usertype_count} records have NULL userType ({NULL_USERTYPE_PERCENTAGE*100}%)")
print(f"-- {deleted_count} records marked as deleted ({DELETED_PERCENTAGE*100}%)")
