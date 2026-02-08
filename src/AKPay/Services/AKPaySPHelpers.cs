using System.Linq;

namespace AKPay.Services;

public partial class AKPaySP : IAKPayService
{
    private const bool INVALID = false;
    private const bool VALID = true;

    private static bool ValidateEmail(string email)
    {
        if(string.IsNullOrWhiteSpace(email) || !email.EndsWith("@lums.edu.pk") ||
            email.Contains(' ') || email.Count(c => c == '@') != 1 || email.Length <= 12)
        {
            return INVALID;
        }
        return VALID;
    }

    private static bool ValidatePhone(string phone)
    {
        if(string.IsNullOrWhiteSpace(phone) || !phone.StartsWith("3") ||
            phone.Contains(" ") || !phone.All(char.IsDigit) || phone.Length != 10)
        {
            return INVALID;
        }
        return VALID;
    }

    private static bool ValidateFullName(string fullName)
    {
        if(string.IsNullOrWhiteSpace(fullName) || !fullName.All(c => char.IsLetter(c) || c == ' '))
        {
            return INVALID;
        }
        return VALID;
    }
}