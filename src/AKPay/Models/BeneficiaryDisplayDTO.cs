using System;
using System.Collections.Generic;

namespace AKPay.Models
{
    public class BeneficiaryDisplayDto
    {
        public string NickName { get; set; } = null!; // nickname given by remitter
        public string BeneficiaryFullName { get; set; } = null!; // their official full name
        public string UserName { get; set; } = null!; // basically like account number in a regular bank
    }
}