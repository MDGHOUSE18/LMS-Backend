using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoanManagementSystem.Core.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
    }

    public interface ILoanRepository : IRepository<LoanApplication>
    {
        Task<IEnumerable<LoanApplication>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<LoanApplication>> GetByStatusAsync(LoanStatus status);
        Task<LoanApplication?> GetWithDetailsAsync(Guid id);
        Task<bool> HasActiveLoanAsync(Guid userId, int days = 30);
    }

    public interface IDocumentRepository : IRepository<Document>
    {
        Task<IEnumerable<Document>> GetByLoanIdAsync(Guid loanId);
        Task<IEnumerable<Document>> GetPendingVerificationAsync();
        Task<Document?> GetByTypeAsync(Guid loanId, DocumentType type);
    }

    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetByLoanIdAsync(Guid loanId);
        Task<IEnumerable<Payment>> GetDuePaymentsAsync(DateTime dueDate);
        Task<IEnumerable<Payment>> GetOverduePaymentsAsync(int daysOverdue);
        Task<decimal> GetOutstandingBalanceAsync(Guid loanId);
    }

    public interface IAuditLogRepository : IRepository<AuditLog>
    {
        Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId);
        Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100);
    }

    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByPhoneAsync(string phone);
        Task<User?> GetWithLoansAsync(Guid userId);
    }
}
