using LMS.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Loan
{
    public class LoanAssignment
    {
        public Guid Id { get; set; }
        public Guid LoanApplicationId { get; set; }
        public Guid AssignedOfficerId { get; set; }
        public DateTime AssignedAt { get; set; }

        // Navigation Properties
        public LoanApplication LoanApplication { get; set; } = null!;
        public User AssignedOfficer { get; set; } = null!;
    }
}
