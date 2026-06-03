using LMS.Application.Interfaces.Repositories.Loan;
using LMS.Domain.Entities.Loan;
using LMS.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Repositories.Loan
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Payment> GetByIdAsync(Guid id)
        {
            return await _context.Payments.FindAsync(id);
        }

        public async Task<List<Payment>> GetAllAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        public void Add(Payment entity)
        {
            _context.Payments.Add(entity);
        }

        public void Update(Payment entity)
        {
            _context.Payments.Update(entity);
        }

        public void Delete(Payment entity)
        {
            _context.Payments.Remove(entity);
        }

        public IUnitOfWork UnitOfWork => _context;

        public async Task<List<Payment>> GetByLoanIdAsync(Guid loanId)
        {
            return await _context.Payments
                .Where(p => p.LoanId == loanId)
                .OrderBy(p => p.EmiMonth)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetOverduePaymentsAsync(DateTime asOfDate)
        {
            return await _context.Payments
                .Where(p => p.DueDate < asOfDate && p.Status == PaymentStatus.Pending)
                .OrderBy(p => p.DueDate)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetUpcomingPaymentsAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Payments
                .Where(p => p.DueDate >= fromDate && p.DueDate <= toDate && p.Status == PaymentStatus.Pending)
                .OrderBy(p => p.DueDate)
                .ToListAsync();
        }
    }
}
