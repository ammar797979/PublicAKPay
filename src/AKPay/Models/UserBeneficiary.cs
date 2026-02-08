using System;
using System.Collections.Generic;

namespace AKPay.Models;

public partial class UserBeneficiary
{
    public int RemitterId { get; set; }

    public int BeneficiaryId { get; set; }

    public string? NickName { get; set; }

    public DateTime? LastPaymentTime { get; set; }

    public decimal? LastPaymentAmount { get; set; }

    public DateTime? AddedAt { get; set; }

    public virtual User Beneficiary { get; set; } = null!;

    public virtual User Remitter { get; set; } = null!;
}
