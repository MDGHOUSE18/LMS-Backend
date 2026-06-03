using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LMS.Application.Interfaces.Services.Loan
{
    public interface IPaymentScheduleService
    {
        Task GenerateScheduleAsync(Guid loanId);
        Task<List<Payment>> GetScheduleAsync(Guid loanId);
        Task MarkAsPaidAsync(Guid paymentId, DateTime paidDate);
        Task MarkAsMissedAsync(Guid paymentId);
        Task<List<Payment>> GetOverduePaymentsAsync(DateTime asOfDate);
    }
}
