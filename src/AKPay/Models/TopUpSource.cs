using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class TopUpSource
{
    public int SourceId { get; set; }

    public string SourceName { get; set; } = null!;

    public virtual ICollection<TopUpTransaction> TopUpTransactions { get; set; } = new List<TopUpTransaction>();
}
