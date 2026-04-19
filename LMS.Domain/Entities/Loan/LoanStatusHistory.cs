using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Loan
{
    public class LoanStatusHistory
    {
        public int Id { get; set; }

        public int LoanApplicationId { get; set; }
        public int? FromStatusId { get; set; }
        public int ToStatusId { get; set; }

        public int ChangedBy { get; set; }
        public DateTime ChangedAt { get; set; }

        public string? Comments { get; set; }
    }
}
