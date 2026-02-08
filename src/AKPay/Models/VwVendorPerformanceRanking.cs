using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class VwVendorPerformanceRanking
{
    public string VendorName { get; set; } = null!;

    public int? TotalTransactions { get; set; }

    public decimal? TotalRevenue { get; set; }

    public long? RankPosition { get; set; }
}
