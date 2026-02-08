using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class VendorStatus1
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public DateTime? TillWhen { get; set; }

    public virtual ICollection<Vendor> Vendors { get; set; } = new List<Vendor>();
}
