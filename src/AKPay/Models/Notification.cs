using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public string RecipientType { get; set; } = null!;

    public int RecipientId { get; set; }

    public string? TxType { get; set; }

    public int? TxId { get; set; }

    public string Msg { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public bool IsRead { get; set; }

    public string NotifType { get; set; } = null!;
}
