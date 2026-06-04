using LMS.Application.Interfaces.Repositories.Loan;
using LMS.Domain.Entities.Loan;
using LMS.Domain.Enums;
using LMS.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Repositories.Loan
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly LMSDbContext _context;

        public PaymentRepository(LMSDbContext context)
        {
            _context = context;
        }

        public async Task<Payment?> GetByIdAsync(Guid id)
        {
            return await _context.Payments.FindAsync(id);
        }

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments.ToListAsync();
        }

        public async Task<IEnumerable<Payment>> FindAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _context.Payments.Where(predicate).ToListAsync();
        }

        public async Task<Payment> AddAsync(Payment entity)
        {
            await _context.Payments.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public void Add(Payment entity)
        {
            _context.Payments.Add(entity);
            _context.SaveChanges();
        }

        public void Update(Payment entity)
        {
            _context.Payments.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(Payment entity)
        {
            _context.Payments.Remove(entity);
            _context.SaveChanges();
        }

        public void Remove(Payment entity)
        {
            _context.Payments.Remove(entity);
            _context.SaveChanges();
        }

        public void RemoveRange(IEnumerable<Payment> entities)
        {
            _context.Payments.RemoveRange(entities);
            _context.SaveChanges();
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Payments.AnyAsync(p => p.Id == id);
        }

        public async Task<int> CountAsync()
        {
            return await _context.Payments.CountAsync();
        }

        public async Task<int> CountAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _context.Payments.CountAsync(predicate);
        }

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
                .Where(p => p.DueDate < asOfDate && p.Status == PaymentStatus.Pending.ToString())
                .OrderBy(p => p.DueDate)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetUpcomingPaymentsAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Payments
                .Where(p => p.DueDate >= fromDate && p.DueDate <= toDate && p.Status == PaymentStatus.Pending.ToString())
                .OrderBy(p => p.DueDate)
                .ToListAsync();
        }
    }
}
