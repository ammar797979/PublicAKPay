using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class VwUserMonthlyStat
{
    public string UserFullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? TransactionMonth { get; set; }

    public decimal? TotalSpent { get; set; }

    public decimal? AvgTransactionSize { get; set; }
}
