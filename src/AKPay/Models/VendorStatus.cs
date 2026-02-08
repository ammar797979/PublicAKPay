using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class VendorStatus
{
    public int VendorId { get; set; }

    public string VendorName { get; set; } = null!;

    public decimal VendorBalance { get; set; }

    public string CurrentStatus { get; set; } = null!;

    public string? ManagerName { get; set; }

    public string? ManagerPhone { get; set; }

    public DateTime LastUpdateTime { get; set; }
}
