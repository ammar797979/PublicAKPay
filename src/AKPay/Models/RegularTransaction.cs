using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class RegularTransaction
{
    public int RegularTransactionId { get; set; }

    public int FromUserId { get; set; }

    public int ToVendorId { get; set; }

    public decimal Amount { get; set; }

    public DateTime TxTimeStamp { get; set; }

    public string PaymentMode { get; set; } = null!;

    public int TxStatusId { get; set; }

    public virtual User FromUser { get; set; } = null!;

    public virtual Vendor ToVendor { get; set; } = null!;

    public virtual TransactionStatus TxStatus { get; set; } = null!;
}
