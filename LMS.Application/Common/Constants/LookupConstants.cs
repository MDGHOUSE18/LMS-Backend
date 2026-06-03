using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Common.Constants
{
    public static class LoanStatus
    {
        public const string Draft = "Draft";
        public const string Submitted = "Submitted";
        public const string UnderReview = "UnderReview";
        public const string Refer = "Refer";
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Disbursed = "Disbursed";
    }

    public static class DocumentType
    {
        public const string Aadhaar = "Aadhaar";
        public const string Pan = "PAN";
        public const string SalarySlip = "SalarySlip";
        public const string BankStatement = "BankStatement";
    }
}
