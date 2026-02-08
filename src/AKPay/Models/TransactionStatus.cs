using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class TransactionStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public string? StatusDescription { get; set; }

    public virtual ICollection<RegularTransaction> RegularTransactions { get; set; } = new List<RegularTransaction>();

    public virtual ICollection<TopUpTransaction> TopUpTransactions { get; set; } = new List<TopUpTransaction>();

    public virtual ICollection<UserToUserTransaction> UserToUserTransactions { get; set; } = new List<UserToUserTransaction>();

    public virtual ICollection<VendorPaymentTransaction> VendorPaymentTransactions { get; set; } = new List<VendorPaymentTransaction>();
}
