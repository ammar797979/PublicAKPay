using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class UserToUserTransaction
{
    public int UtoUtransactionId { get; set; }

    public int ToUserId { get; set; }

    public int FromUserId { get; set; }

    public decimal Amount { get; set; }

    public DateTime TxTimeStamp { get; set; }

    public int TxStatusId { get; set; }

    public virtual User FromUser { get; set; } = null!;

    public virtual User ToUser { get; set; } = null!;

    public virtual TransactionStatus TxStatus { get; set; } = null!;
}
