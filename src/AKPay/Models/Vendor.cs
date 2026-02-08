using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class Vendor
{
    public int VendorId { get; set; }

    public string VendorName { get; set; } = null!;

    public decimal VendorBalance { get; set; }

    public DateTime LastUpdateTime { get; set; }

    public string? ManagerName { get; set; }

    public string? ManagerPhone { get; set; }

    public int StatusId { get; set; }

    public virtual ICollection<RegularTransaction> RegularTransactions { get; set; } = new List<RegularTransaction>();

    public virtual VendorStatus1 Status { get; set; } = null!;

    public virtual ICollection<VendorPaymentTransaction> VendorPaymentTransactions { get; set; } = new List<VendorPaymentTransaction>();
}
