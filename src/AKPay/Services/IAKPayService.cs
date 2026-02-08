using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AKPay.Models;

namespace AKPay.Services
{
    public interface IAKPayService
    {
        // Validates email then queries Users DB for it
        // Returns 1 if user in DB, 0 if not, -1 if in DB but isDeleted is set to 1
        // -2 for other errors like invalid input
        Task<int> IsUserRegistered(string email);

        // Validates inputs as per constraints
        // Inserts to Users DB if valid else returns null
        Task<User?> CreateUser(string email, string phone, string fullName, string passwordHash, string userType = "student");

        // 1.2 Validates Email then checks if email in Users table
        // If yes, compares the input passwordHash to the passwordHash stored for the User with that email
        // ONLY if not deleted, otherwise error output
        Task<User?> AuthUser(string email, string passwordHash);

        // Validates email then calls upon third-party (write TODO or TP-API at this point) and throw notImplemented
        Task<int> InitPassReset(string email);

        // Share button will throw notImplemented

        // Tx History, IDEK we will look at this last when slightly more comfortable with stuff

        // For testing we will show regular Tx with randomized values
        // When regular Tx screen is opened, we will show a button saying generate invoice
        // This will move to the next screen (3.1)
        Task<bool> AsyncRegularTx(decimal amount, int fromUserID, int toVendorID);

        // 4.1 --> 4.11 (user details button)
        // Validates input then
        // Fetches user details for beneficiary mgmt (view?)
        Task<User?> FetchBenfDetails(string userName);

        // Add beneficiary against userID to the beneficiaries table
        // Ensure the beneficiary being added isn't already in the table against this user
        Task<bool> CreateBeneficiary(int remitterID, string beneficiaryUsername, string? nickName = null);

        // Fetch List of Beneficiaries of remitter for 4.0
        Task<List<BeneficiaryDisplayDto>> FetchRemittersBeneficiaries(int remitterID);

        // 4.21 U2U makes relevant changes in the accounts of each user, and does general tx validation as well
        Task<bool> AsyncU2UTx(decimal amount, int toUserID, int fromUserID);

        // Share button will throw notImplemented

        // Save SS button will throw notImplemented

        // 7.1 Validates old passwordHash, and checks if new passwordHash and retypedPasswordHash are both the same
        // If valid request, updates passwordHash of user, can call VerifyPassword() (7.3) too or AuthUser (1.2)
        Task<bool> ChangePassword(string email, string currentHash, string newHash, string retypeHash);

        // 7.3 Validates passwordHash (confirmation should be taken on UI again even if password correct)
        // Might be same as AuthUser (1.2)
        // Task<bool> VerifyPassword(string passwordHash);

        // Add any kind of notification sent to "User" or "Vendor" to notifs table
        // (recipientType constraint to be either of those two)
        // notifTypes: "Success", "Failure", "Info"
        // If notifType is info, then TxID and TxType can be NULL, otherwise must have value
        // TxType if not null, it can be either "Regular", "U2U", "TopUp", "VendorPay"
        // Returns True if successfully added else False
        Task<bool> Notified(string recipientType = "User", int recipientID = 0, string msg = "", string notifType = "Info", string? txType = null, int? txID = null);
    }
}