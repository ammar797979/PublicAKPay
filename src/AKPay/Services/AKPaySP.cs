using System;
using System.Data;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using AKPay.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace AKPay.Services;

public partial class AKPaySP : IAKPayService
{
    private readonly AkpayDbContext _db;

    public AKPaySP(AkpayDbContext db)
    {
        _db = db;
    }

    public async Task<int> IsUserRegistered(string email)
    {
        if (!ValidateEmail(email))
            return -2;

        var spEmail = new SqlParameter("@Email", email);
        var returnValue = new SqlParameter("@result", SqlDbType.Int) // for int value to be returned
        {
            Direction = ParameterDirection.Output
        };

        try
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC dbo.IsUserRegistered
                @Email = {spEmail},
                @result = {returnValue} OUTPUT");

            return (int)returnValue.Value; // return int value: 1 Active, 0 not found, -1 deleted
        }

        catch(Exception ex)
        {
            Console.WriteLine($"IsUserRegistered system error: {ex.Message}");
            return -99;
        }
        // throw new NotImplementedException();
    }
    public async Task<User?> CreateUser(string email, string phone, string fullName, string passwordHash, string userType = "student")
    {
        // comments for Fajr
        // this method was supposed to be a LINQ implementation
        // my approach was to do as much in LINQ as possible, and never call sproc
        // due to our triggers i had no choice, so i had to call sproc
        // your approach should be reverse, only sproc unless absolutely necessry to use LINQ
        // so all this csharp mai validation using helpers and stuff you see, you can omit it since your sproc are prolly doing it
        // and the below sproc part you see is how you call an sproc from LINQ
        // your primary goal should be that this entire function works in your implementation with as less csharp as possible
        if(string.IsNullOrWhiteSpace(passwordHash)){
                return null;
        }
        // bool validEmail = ValidateEmail(email);
        // bool validPhone = ValidatePhone(phone);
        // bool validFullName = ValidateFullName(fullName);
        // if(!validEmail || !validPhone || !validFullName)
        // {
            // return null;
        // }
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
            Console.WriteLine($"SP_CreateUser(Sproc): {ex.Message}");
            return null;
        }
    }

    public async Task<User?> AuthUser(string email, string passwordHash)
    {
        if (!ValidateEmail(email) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return null;
        }

        var connection = _db.Database.GetDbConnection();
        var openedHere = connection.State != ConnectionState.Open;

        await using var command = connection.CreateCommand();
        command.CommandText = "dbo.LoginUser";
        command.CommandType = CommandType.StoredProcedure;

        var spEmail = new SqlParameter("@Email", SqlDbType.VarChar, 100) { Value = email };
        var spPasswordHash = new SqlParameter("@PasswordHash", SqlDbType.VarChar, 255) { Value = passwordHash };
        var spResult = new SqlParameter("@result", SqlDbType.Int)
        {
            Direction = ParameterDirection.Output
        };

        command.Parameters.Add(spEmail);
        command.Parameters.Add(spPasswordHash);
        command.Parameters.Add(spResult);

        User? resolvedUser = null;
        int? resolvedUserId = null;

        try
        {
            if (openedHere)
            {
                await connection.OpenAsync();
            }

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                resolvedUserId = GetInt(reader, "userID");

                resolvedUser = new User
                {
                    UserId = resolvedUserId ?? 0,
                    Email = GetString(reader, "email") ?? string.Empty,
                    Phone = GetString(reader, "phone") ?? string.Empty,
                    FullName = GetString(reader, "fullName") ?? string.Empty,
                    PasswordHash = GetString(reader, "passwordHash") ?? string.Empty,
                    UserType = GetString(reader, "userType"),
                    DateCreated = GetDateTime(reader, "dateCreated") ?? DateTime.MinValue,
                    IsDeleted = GetBool(reader, "isDeleted") ?? false,
                    DeletedAt = GetDateTime(reader, "deletedAt"),
                    UserName = GetString(reader, "userName")
                };
            }

            await reader.CloseAsync();

            var resultCode = spResult.Value is int code ? code : -99;

            if (resultCode != 0)
            {
                Console.WriteLine($"AuthUser failed with code: {resultCode}. Email: {email}");
                return null;
            }

            if (resolvedUser == null && resolvedUserId.HasValue)
            {
                resolvedUser = await _db.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == resolvedUserId.Value);
            }

            return resolvedUser;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AuthUser system error: {ex.Message}");
            return null;
        }
        finally
        {
            if (openedHere && connection.State == ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }

        static int? GetInt(DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        static string? GetString(DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        static bool? GetBool(DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                {
                    return null;
                }

                var value = reader.GetValue(ordinal);
                return value switch
                {
                    bool b => b,
                    byte bt => bt != 0,
                    short s => s != 0,
                    int i => i != 0,
                    _ => null
                };
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }

        static DateTime? GetDateTime(DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
        }
    }

    public Task<int> InitPassReset(string email)
    {
        // Since OTP not implemented, we use ResetPassword directly. Exception thrown.
        throw new NotImplementedException();
    }

    public async Task<bool> AsyncRegularTx(decimal amount, int fromUserID, int toVendorID)
    {
        if(amount <= 0) return false;

        var spUserID = new SqlParameter("@UserID", fromUserID);
        var spVendorID = new SqlParameter("@VendorID", toVendorID);
        var spAmount = new SqlParameter("@Amount", amount);

        try
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC dbo.Regular_tx
                @UserID = {spUserID},
                @VendorID = {spVendorID},
                @Amount = {spAmount}
            ");

            Console.WriteLine($"Successfully processed amount {amount} from UserID {fromUserID} to VendorID {toVendorID}.");
            return true;
        }

        catch (SqlException ex) when (ex.Number == 50003)
        {
            Console.WriteLine($"AsyncRegularTx failed (Insufficient balance): {ex.Message}");
            return false;
        }

        catch(SqlException ex)
        {
            Console.WriteLine($"AsyncRegularTx failed (DB Error): {ex.Message}");
            return false;  
        }
        // throw new NotImplementedException();
    }

    public async Task<User?> FetchBenfDetails(string userName)
    {
        if(string.IsNullOrWhiteSpace(userName))
        {
            return null;
        }

        var spSearch = new SqlParameter("@SearchWord", userName);

        try
        {
            var beneficiary = await _db.Users
                .FromSqlInterpolated($@"
                    EXEC dbo.SearchUser
                    @SearchWord = {spSearch}
                ")
                .ToListAsync();

            return beneficiary.FirstOrDefault();
        }
        
        catch(Exception ex)
        {
            Console.WriteLine($"Error executing dbo.SearchUser: {ex.Message}");
            return null;
        }
        
        // throw new NotImplementedException();
    }

    public async Task<bool> CreateBeneficiary(int remitterID, string beneficiaryUsername, string? nickName = null)
    {
        var spRemitterID = new SqlParameter("@RemitterID", remitterID);
        var spBeneficiaryUsername = new SqlParameter("@BeneficiaryUsername", beneficiaryUsername);
        var spNickName = new SqlParameter("@Nickname", (object?)nickName ?? DBNull.Value);

        try
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC dbo.AddBeneficiary
                @RemitterID = {spRemitterID},
                @BeneficiaryUsername = {spBeneficiaryUsername},
                @Nickname = {spNickName}
            ");

            return true;
        }
        catch{
            return false;
        }
    }

    public async Task<List<BeneficiaryDisplayDto>> FetchRemittersBeneficiaries(int remitterID)
    {
        var spRemitterID = new SqlParameter("@RemitterID", remitterID);

        try
        {
            var beneficiaries = await _db.Database.SqlQueryRaw<BeneficiaryDisplayDto>(
                "EXEC dbo.ListBeneficiaries @RemitterID = {0}",
                spRemitterID.Value)
                .ToListAsync();

            return beneficiaries;
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Error listing beneficiaries: {ex.Message}");
            return new List<BeneficiaryDisplayDto>();
        }
        // throw new NotImplementedException();
    }

    public async Task<bool> AsyncU2UTx(decimal amount, int toUserID, int fromUserID)
    {
        if (amount <= 0 || fromUserID == toUserID) return false;

        var spFromUserID = new SqlParameter("@FromUserID", fromUserID);
        var spToUserID = new SqlParameter("@ToUserID", toUserID);
        var spAmount = new SqlParameter("@Amount", amount);

        try
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC dbo.U2U_tx
                @FromUserID = {spFromUserID},
                @ToUserID = {spToUserID},
                @Amount = {spAmount}
            ");

            return true;
        }

        catch(SqlException ex) when (ex.Number >= 50000 && ex.Number <= 51006)
        {
            Console.WriteLine($"AsyncU2UTx failed (SP Error): {ex.Message}");
            return false;
        }

        catch(SqlException ex)
        {
            Console.WriteLine($"AsyncU2UTx failed (DB Error): {ex.Message}");
            return false;
        }
        // throw new NotImplementedException();
    }

    public async Task<bool> ChangePassword(string email, string currentHash, string newHash, string retypeHash)
    {
        if(newHash != retypeHash)
        {
            Console.WriteLine("New password and retype password hashes do not match.");
            return false;
        }
    
        if(!ValidateEmail(email) || string.IsNullOrWhiteSpace(newHash)) return false;
        
        var spEmail = new SqlParameter("@Email", email);
        var spNewHash = new SqlParameter("@NewPasswordHash", newHash);

        try
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC dbo.ResetPassword
                @Email = {spEmail},
                @NewPasswordHash = {spNewHash}
            ");

            return true;
        }

        catch (SqlException ex)
        {
            Console.WriteLine($@"sp_ResetPassword error: {ex.Message}");
            return false;
        }
        // throw new NotImplementedException();
    }

    public async Task<bool> Notified(string recipientType = "User", int recipientID = 0, string msg = " ", string notifType = "Info", string? txType = null, int? txID = null)
    {
        var spRecipientType = new SqlParameter("@RecipientType", recipientType);
        var spRecipientID = new SqlParameter("@RecipientID", recipientID);
        var spMsg = new SqlParameter("@Msg", msg);
        var spNotifType = new SqlParameter("@NotifType", notifType);
        var spTxType = new SqlParameter("@TxType", (object?)txType ?? DBNull.Value);
        var spTxID = new SqlParameter("@TxID", (object?)txID ?? DBNull.Value);
        var returnValue = new SqlParameter("@result", SqlDbType.Int)
        {
            Direction = ParameterDirection.ReturnValue
        };

        try
        {
            await _db.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC {returnValue} = dbo.LogNotifications
                    @RecipientType = {spRecipientType},
                    @RecipientID = {spRecipientID},
                    @Msg = {spMsg},
                    @NotifType = {spNotifType},
                    @TxType = {spTxType},
                    @TxID = {spTxID}
                ");
            return (int)returnValue.Value == 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"LogNotifications system error: {ex.Message}");
            return false;
        }
    }
}
