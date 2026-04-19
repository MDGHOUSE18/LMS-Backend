using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Repositories.Loan
{
    public interface ILoanRepository
    {
        Task<LoanApplication> CreateAsync(LoanApplication loan);
        Task<LoanApplication?> GetByIdAsync(int id);
        Task UpdateAsync(LoanApplication loan);
    }
}
