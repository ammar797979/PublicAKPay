using System;
using System.Collections.Generic;

namespace AKPay.Models;

public class TransactionHistoryRecord
{
    public string TxType {get; set;} = string.Empty;
    public int? SenderID {get; set;}
    public string? SenderName {get; set;}
    public int ReceiverID {get; set;}
    public string? ReceiverName {get; set;}
    public decimal Amount {get; set;}
    public DateTime TxTimeStamp {get; set;}
    public string? PaymentMode {get; set;}
    public string Sign {get; set;} = string.Empty;
}
