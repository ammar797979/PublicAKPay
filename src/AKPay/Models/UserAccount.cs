using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class UserAccount
{
    public int UserId { get; set; }

    public decimal UserBalance { get; set; }

    public DateTime LastUpdateTime { get; set; }

    public bool IsActive { get; set; }

    public virtual User User { get; set; } = null!;
}
