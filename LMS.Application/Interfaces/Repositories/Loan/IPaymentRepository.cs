using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Repositories.Loan
{
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<List<Payment>> GetByLoanIdAsync(Guid loanId);
        Task<List<Payment>> GetOverduePaymentsAsync(DateTime asOfDate);
        Task<List<Payment>> GetUpcomingPaymentsAsync(DateTime fromDate, DateTime toDate);
        void Add(Payment entity);
        void Delete(Payment entity);
    }
}
