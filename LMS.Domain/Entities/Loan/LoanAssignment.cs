using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Loan
{
    public class LoanAssignment
    {
        public int Id { get; set; }
        public int LoanApplicationId { get; set; }
        public int AssignedOfficerId { get; set; }
        public DateTime AssignedAt { get; set; }
    }
}
