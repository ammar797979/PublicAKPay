using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AKPay.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace AKPay.Services;

public partial class AKPayLINQ : IAKPayService
{
    private readonly AkpayDbContext _db;

    public AKPayLINQ(AkpayDbContext db)
    {
        _db = db;
    }

    public async Task<int> IsUserRegistered(string email)
    {
        if(!ValidateEmail(email))
        {
            return -2;
        }
        string usernamePart = email.Substring(0, email.IndexOf('@'));
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == usernamePart);
        if(user == null)
        {
            return 0;
        }
        if(user.IsDeleted)
        {
            return -1;
        }
        return 1;
    }

    public async Task<User?> CreateUser(string email, string phone, string fullName, string passwordHash, string userType = "student")
    {
        if(string.IsNullOrWhiteSpace(passwordHash)){
                return null;
        }
        bool validEmail = ValidateEmail(email);
        bool validPhone = ValidatePhone(phone);
        bool validFullName = ValidateFullName(fullName);
        if(!validEmail || !validPhone || !validFullName)
        {
            return null;
        }
        var existing = await _db.Users.FirstOrDefaultAsync(u => (u.Email == email || u.Phone == phone) && !u.IsDeleted);
        if(existing != null){
            return null;
        }

        // MUST call sproc due to instead of direct insert triggers
        // doing in LINQ is a safety hazard to turn off triggers for all sessions
        var spEmail = new SqlParameter("@email", email);
        var spPhone = new SqlParameter("@phone", phone);
        var spFullName = new SqlParameter("@fullName", fullName);
        var spPasswordHash = new SqlParameter("@passwordHash", passwordHash);
        var spUserType = new SqlParameter("@userType", userType);
        var spUserID = new SqlParameter("userID", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };
        try
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC SP_CreateUser
                    @email = {spEmail}, 
                    @phone = {spPhone}, 
                    @fullName = {spFullName}, 
                    @passwordHash = {spPasswordHash}, 
                    @userType = {spUserType}, 
                    @userID = {spUserID} OUTPUT
            ");
            int? newUserID = (int?)spUserID.Value;
            if(newUserID == null)
            {
                return null;
            }
            var newUser = await _db.Users.Where(u => u.UserId == newUserID).FirstOrDefaultAsync();
            return newUser;
        }
        catch(SqlException ex)
        {
            // log exception
            Console.WriteLine($"SP_CreateUser(LINQ): {ex.Message}");
            return null;
        }
    }

    public async Task<User?> AuthUser(string email, string passwordHash)
    {
        bool validEmail = ValidateEmail(email);
        if (!validEmail || string.IsNullOrWhiteSpace(passwordHash))
        {
            return null;
        }
        string usernamePart = email.Substring(0, email.IndexOf('@'));
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == usernamePart && !u.IsDeleted);
        if (user == null)
        {
            return null;
        }
        return user.PasswordHash == passwordHash ? user : null;
    }

    public Task<int> InitPassReset(string email)
    {
        Console.WriteLine("Password resetting for FORGOT password will be through email so not implemented in our app");
        throw new NotImplementedException();
    }

    public async Task<bool> AsyncRegularTx(decimal amount, int fromUserID, int toVendorID)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var user = _db.Users.Find(fromUserID);
            var vendor = _db.Vendors.Find(toVendorID);
            var userAcc = await _db.UserAccounts.FromSqlRaw($"SELECT * FROM UserAccounts WITH (UPDLOCK) WHERE userID = {{0}}", fromUserID).FirstOrDefaultAsync();
            if(user == null || vendor == null || userAcc == null || amount <= 0 || user.IsDeleted || !userAcc.IsActive || userAcc.UserBalance < amount)
            {
                return false;
            }
            userAcc.UserBalance -= amount;
            vendor.VendorBalance += amount;
            await _db.SaveChangesAsync();
            var regTx = new RegularTransaction
            {
                Amount = amount,
                FromUserId = fromUserID,
                ToVendorId = toVendorID
            };
            _db.RegularTransactions.Add(regTx);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            Console.WriteLine($"Regular Tx of amount {amount} from UserID {fromUserID} to VendorID {toVendorID} processed");
            return true;
        }
        catch(Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"AsyncRegularTx(LINQ): {ex.Message}");
            return false;
        }
    }

    public async Task<User?> FetchBenfDetails(string userName)
    {
        if(string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }
        return await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == userName && !u.IsDeleted);
    }

    public async Task<bool> CreateBeneficiary(int remitterID, string beneficiaryUsername, string? nickName = null)
    {
        if(remitterID <= 0 || string.IsNullOrWhiteSpace(beneficiaryUsername))
        {
            return false;
        }
        beneficiaryUsername = beneficiaryUsername.Trim();
        var beneficiary = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserName == beneficiaryUsername && !u.IsDeleted);
        var remitter = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == remitterID && !u.IsDeleted);
        if(beneficiary == null || remitter == null)
        {
            return false;
        }
        if(remitterID == beneficiary.UserId)
        {
            return false;
        }
        var alreadyExists = await _db.UserBeneficiaries.AnyAsync(row => row.RemitterId == remitterID && row.BeneficiaryId == beneficiary.UserId);
        if(alreadyExists){
            return false;
        }
        if(string.IsNullOrWhiteSpace(nickName))
        {
            nickName = beneficiary.FullName;
        }
        else
        {
            nickName = nickName.Trim();
        }
        var newRemitBenefCombo = new UserBeneficiary
        {
            RemitterId = remitterID,
            BeneficiaryId = beneficiary.UserId,
            NickName = nickName,
        };
        await _db.UserBeneficiaries.AddAsync(newRemitBenefCombo);
        try
        {
            await _db.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException ex)
        {
            var entry = _db.Entry(newRemitBenefCombo);
            if (entry != null)
            {
                entry.State = EntityState.Detached;
            }
            Console.WriteLine($"CreateBeneficiary(LINQ) failed: {ex.InnerException?.Message ?? ex.Message}");
            return false;
        }
    }

    public async Task<List<BeneficiaryDisplayDto>> FetchRemittersBeneficiaries(int remitterID)
    {
        if(remitterID <= 0)
        {
            return new List<BeneficiaryDisplayDto>();
        }
        return await _db.UserBeneficiaries.AsNoTracking()
            .Where(b => b.RemitterId == remitterID)
            .Select(b => new BeneficiaryDisplayDto
            {
                NickName = b.NickName ?? b.Beneficiary.FullName,
                BeneficiaryFullName = b.Beneficiary.FullName,
                UserName = b.Beneficiary.UserName ?? b.Beneficiary.Email.Substring(0, b.Beneficiary.Email.IndexOf('@')),
            })
            .ToListAsync();
    }

    public async Task<bool> AsyncU2UTx(decimal amount, int toUserID, int fromUserID)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var fromUser = _db.Users.Find(fromUserID);
            var toUser = _db.Users.Find(toUserID);
            var fromUserAcc = await _db.UserAccounts.FromSqlRaw($"SELECT * FROM UserAccounts WITH (UPDLOCK) WHERE userID = {{0}}", fromUserID).FirstOrDefaultAsync();
            var toUserAcc = await _db.UserAccounts.FromSqlRaw($"SELECT * FROM UserAccounts WITH (UPDLOCK) WHERE userID = {{0}}", toUserID).FirstOrDefaultAsync();
            if(fromUser == null || toUser == null || fromUserAcc == null || toUserAcc == null ||
                    fromUser.IsDeleted || !fromUserAcc.IsActive || toUser.IsDeleted || !toUserAcc.IsActive ||
                    amount <= 0 || fromUserAcc.UserBalance < amount)
            {
                return false;
            }
            fromUserAcc.UserBalance -= amount;
            toUserAcc.UserBalance += amount;
            var u2uTx = new UserToUserTransaction
            {
                Amount = amount,
                FromUserId = fromUserID,
                ToUserId = toUserID
            };
            _db.UserToUserTransactions.Add(u2uTx);
            var link = await _db.UserBeneficiaries.FirstOrDefaultAsync(b => b.RemitterId == fromUserID && b.BeneficiaryId == toUserID);
            if (link != null)
            {
                link.LastPaymentAmount = amount;
                link.LastPaymentTime = DateTime.UtcNow;
            }
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            Console.WriteLine($"UserToUser Tx of amount {amount} from UserID {fromUserID} to UserID {toUserID} processed");
            return true;
        }
        catch(Exception ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine($"AsyncUserToUserTx(LINQ): {ex.Message}");
            return false;
        }
    }

    public Task<bool> ChangePassword(string email, string currentHash, string newHash, string retypeHash)
    {
        bool validEmail = ValidateEmail(email);
        if(currentHash == newHash || newHash != retypeHash || !validEmail)
        {
            return Task.FromResult(false);
        }
        string usernamePart = email.Substring(0, email.IndexOf('@'));
        var user = _db.Users.FirstOrDefault(u => u.UserName == usernamePart && !u.IsDeleted && u.PasswordHash == currentHash);
        if(user == null)
        {
            return Task.FromResult(false);
        }
        user.PasswordHash = newHash;
        _db.SaveChanges();
        return Task.FromResult(true);
    }

    public async Task<bool> Notified(string recipientType = "User", int recipientID = 0, string msg = " ", string notifType = "Info", string? txType = null, int? txID = null)
    {
        if(recipientType != "User" || recipientType != "Vendor" || recipientID <= 0 || string.IsNullOrWhiteSpace(msg))
        {
            return false;
        }
        if(notifType != "Info" || (notifType != "Success" && txID != null && txType != null) || (notifType != "Failure" && txID != null && txType != null))
        {
            return false;
        }
        if(txType != null)
        {
            if(txType != "Regular" && txType != "U2U" && txType != "TopUp" && txType != "VendorPay")
            {
                return false;
            }
        }
        var notif = new Notification
        {
            RecipientType = recipientType,
            RecipientId = recipientID,
            TxType = txType,
            TxId = txID,
            Msg = msg,
            NotifType = notifType,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notif);
        await _db.SaveChangesAsync();
        return true;
    }
}
