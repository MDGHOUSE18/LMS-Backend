using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Common.Constants
{
    public static class LoanStatusConstants
    {
        public const int Draft = 1;
        public const int Submitted = 2;
        public const int KycPending = 3;
        public const int ReferBack = 4;
    }
}
