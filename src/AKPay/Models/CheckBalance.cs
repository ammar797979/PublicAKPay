using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class CheckBalance
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public decimal Balance { get; set; }

    public DateTime LastUpdateTime { get; set; }
}
