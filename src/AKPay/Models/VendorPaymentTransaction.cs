using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class VendorPaymentTransaction
{
    public int VendorPaymentTransactionId { get; set; }

    public int ToVendorId { get; set; }

    public decimal Amount { get; set; }

    public DateTime TxTimeStamp { get; set; }

    public int TxStatusId { get; set; }

    public virtual Vendor ToVendor { get; set; } = null!;

    public virtual TransactionStatus TxStatus { get; set; } = null!;
}
