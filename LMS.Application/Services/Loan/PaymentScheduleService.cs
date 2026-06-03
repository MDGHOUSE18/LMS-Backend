using LMS.Application.Interfaces.Services.Loan;
using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Application.Services.Loan
{
    public interface IPaymentScheduleService
    {
        Task GenerateScheduleAsync(Guid loanId);
        Task<List<Payment>> GetScheduleAsync(Guid loanId);
        Task MarkAsPaidAsync(Guid paymentId, DateTime paidDate);
        Task MarkAsMissedAsync(Guid paymentId);
        Task<List<Payment>> GetOverduePaymentsAsync(DateTime asOfDate);
    }

    public class PaymentScheduleService : IPaymentScheduleService
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IPaymentRepository _paymentRepository;

        public PaymentScheduleService(ILoanRepository loanRepository, IPaymentRepository paymentRepository)
        {
            _loanRepository = loanRepository;
            _paymentRepository = paymentRepository;
        }

        public async Task GenerateScheduleAsync(Guid loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);
            if (loan == null) throw new ArgumentException("Loan not found", nameof(loanId));

            if (!loan.ApprovedDate.HasValue)
                throw new InvalidOperationException("Cannot generate schedule for unapproved loan");

            // Clear existing schedule if any (regeneration scenario)
            var existingPayments = await _paymentRepository.GetByLoanIdAsync(loanId);
            foreach (var payment in existingPayments)
            {
                _paymentRepository.Delete(payment);
            }

            var payments = new List<Payment>();
            decimal outstandingPrincipal = loan.FinancialDetails!.LoanAmount;
            double monthlyRate = loan.FinancialDetails.InterestRate.Value / 12 / 100;
            int tenureMonths = loan.FinancialDetails.TenureMonths;
            decimal monthlyEmi = loan.FinancialDetails.CalculatedEmi;
            DateTime nextDueDate = loan.ApprovedDate.Value.AddMonths(1);

            for (int month = 1; month <= tenureMonths; month++)
            {
                decimal interestComponent = (decimal)(outstandingPrincipal * (decimal)monthlyRate);
                decimal principalComponent = monthlyEmi - interestComponent;

                // Adjust last month for rounding errors
                if (month == tenureMonths)
                {
                    principalComponent = outstandingPrincipal;
                    monthlyEmi = principalComponent + interestComponent;
                }

                payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    LoanId = loanId,
                    EmiMonth = month,
                    DueDate = nextDueDate,
                    Amount = monthlyEmi,
                    PrincipalPart = principalComponent,
                    InterestPart = interestComponent,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                });

                outstandingPrincipal -= principalComponent;
                nextDueDate = nextDueDate.AddMonths(1);
            }

            foreach (var payment in payments)
            {
                _paymentRepository.Add(payment);
            }

            await _paymentRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task<List<Payment>> GetScheduleAsync(Guid loanId)
        {
            var payments = await _paymentRepository.GetByLoanIdAsync(loanId);
            return payments.OrderBy(p => p.EmiMonth).ToList();
        }

        public async Task MarkAsPaidAsync(Guid paymentId, DateTime paidDate)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null) throw new ArgumentException("Payment not found", nameof(paymentId));

            payment.Status = PaymentStatus.Paid;
            payment.PaidDate = paidDate;
            
            _paymentRepository.Update(payment);
            await _paymentRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task MarkAsMissedAsync(Guid paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null) throw new ArgumentException("Payment not found", nameof(paymentId));

            payment.Status = PaymentStatus.Missed;
            
            _paymentRepository.Update(payment);
            await _paymentRepository.UnitOfWork.SaveChangesAsync();
        }

        public async Task<List<Payment>> GetOverduePaymentsAsync(DateTime asOfDate)
        {
            var allPayments = await _paymentRepository.GetAllAsync();
            return allPayments
                .Where(p => p.DueDate < asOfDate && p.Status == PaymentStatus.Pending)
                .OrderBy(p => p.DueDate)
                .ToList();
        }
    }
}
