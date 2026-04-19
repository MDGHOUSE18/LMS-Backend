using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Loan
{
    public class LoanApplication
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public int StatusId { get; set; }

        public int? PurposeId { get; set; }
        public int? EmploymentTypeId { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        public int CreatedBy { get; set; }
        public int? LastModifiedBy { get; set; }
    }
}
