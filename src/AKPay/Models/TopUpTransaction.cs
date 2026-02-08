using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class TopUpTransaction
{
    public int TopUpTransactionId { get; set; }

    public int SourceId { get; set; }

    public int ToUserId { get; set; }

    public decimal Amount { get; set; }

    public DateTime TxTimeStamp { get; set; }

    public int TxStatusId { get; set; }

    public virtual TopUpSource Source { get; set; } = null!;

    public virtual User ToUser { get; set; } = null!;

    public virtual TransactionStatus TxStatus { get; set; } = null!;
}
