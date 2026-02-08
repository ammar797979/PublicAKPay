using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? UserType { get; set; }

    public DateTime DateCreated { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? UserName { get; set; }

    public virtual ICollection<RegularTransaction> RegularTransactions { get; set; } = new List<RegularTransaction>();

    public virtual ICollection<TopUpTransaction> TopUpTransactions { get; set; } = new List<TopUpTransaction>();

    public virtual UserAccount? UserAccount { get; set; }

    public virtual ICollection<UserBeneficiary> UserBeneficiaryBeneficiaries { get; set; } = new List<UserBeneficiary>();

    public virtual ICollection<UserBeneficiary> UserBeneficiaryRemitters { get; set; } = new List<UserBeneficiary>();

    public virtual ICollection<UserToUserTransaction> UserToUserTransactionFromUsers { get; set; } = new List<UserToUserTransaction>();

    public virtual ICollection<UserToUserTransaction> UserToUserTransactionToUsers { get; set; } = new List<UserToUserTransaction>();
}
