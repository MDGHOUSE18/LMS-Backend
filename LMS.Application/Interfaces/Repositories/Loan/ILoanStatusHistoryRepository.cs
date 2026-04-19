using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Repositories.Loan
{
    public interface ILoanStatusHistoryRepository
    {
        Task AddAsync(LoanStatusHistory history);
    }
}
