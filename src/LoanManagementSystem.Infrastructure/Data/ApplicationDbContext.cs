using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Core.Entities;
using LoanManagementSystem.Core.Interfaces;

namespace LoanManagementSystem.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<LoanApplication> Loans { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(15);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            });

            // LoanApplication configuration
            modelBuilder.Entity<LoanApplication>(entity =>
            {
                entity.HasKey(e => e.LoanId);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.TenureMonths).IsRequired();
                entity.Property(e => e.AnnualInterestRate).IsRequired().HasColumnType("decimal(5,2)");
                entity.Property(e => e.Status).IsRequired();
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Loans)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Document configuration
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.DocId);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.S3Path).IsRequired();
                entity.Property(e => e.VerificationStatus).IsRequired();
                entity.HasOne(e => e.Loan)
                      .WithMany(l => l.Documents)
                      .HasForeignKey(e => e.LoanId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Payment configuration
            modelBuilder.Entity<Payment>(entity =>
            {
                entity.HasKey(e => e.PaymentId);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.DueDate).IsRequired();
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.PrincipalPart).HasColumnType("decimal(18,2)");
                entity.Property(e => e.InterestPart).HasColumnType("decimal(18,2)");
                entity.HasOne(e => e.Loan)
                      .WithMany(l => l.Payments)
                      .HasForeignKey(e => e.LoanId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // AuditLog configuration
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.LogId);
                entity.Property(e => e.Action).IsRequired().HasMaxLength(50);
                entity.Property(e => e.EntityType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Details).HasMaxLength(4000);
                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.UserId);
            });
        }
    }

    public class GenericRepository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public class LoanRepository : GenericRepository<LoanApplication>, ILoanRepository
    {
        public LoanRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<LoanApplication>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet.Where(l => l.UserId == userId)
                               .OrderByDescending(l => l.AppliedDate)
                               .ToListAsync();
        }

        public async Task<IEnumerable<LoanApplication>> GetByStatusAsync(LoanStatus status)
        {
            return await _dbSet.Where(l => l.Status == status)
                               .OrderByDescending(l => l.AppliedDate)
                               .ToListAsync();
        }

        public async Task<LoanApplication?> GetWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(l => l.Documents)
                .Include(l => l.User)
                .FirstOrDefaultAsync(l => l.LoanId == id);
        }

        public async Task<bool> HasActiveLoanAsync(Guid userId, int days = 30)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-days);
            return await _dbSet.AnyAsync(l => 
                l.UserId == userId && 
                (l.Status == LoanStatus.Submitted || 
                 l.Status == LoanStatus.UnderReview || 
                 l.Status == LoanStatus.Approved ||
                 l.Status == LoanStatus.Disbursed) &&
                l.AppliedDate >= cutoffDate);
        }
    }

    public class DocumentRepository : GenericRepository<Document>, IDocumentRepository
    {
        public DocumentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Document>> GetByLoanIdAsync(Guid loanId)
        {
            return await _dbSet.Where(d => d.LoanId == loanId)
                               .OrderBy(d => d.Type)
                               .ToListAsync();
        }

        public async Task<IEnumerable<Document>> GetPendingVerificationAsync()
        {
            return await _dbSet.Where(d => d.VerificationStatus == VerificationStatus.Pending)
                               .OrderBy(d => d.UploadedAt)
                               .ToListAsync();
        }

        public async Task<Document?> GetByTypeAsync(Guid loanId, DocumentType type)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.LoanId == loanId && d.Type == type);
        }
    }

    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Payment>> GetByLoanIdAsync(Guid loanId)
        {
            return await _dbSet.Where(p => p.LoanId == loanId)
                               .OrderBy(p => p.EmiMonth)
                               .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetDuePaymentsAsync(DateTime dueDate)
        {
            return await _dbSet.Where(p => p.DueDate.Date == dueDate.Date && p.Status == PaymentStatus.Pending)
                               .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetOverduePaymentsAsync(int daysOverdue)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-daysOverdue);
            return await _dbSet.Where(p => p.DueDate < cutoffDate && p.Status == PaymentStatus.Pending)
                               .OrderBy(p => p.DueDate)
                               .ToListAsync();
        }

        public async Task<decimal> GetOutstandingBalanceAsync(Guid loanId)
        {
            var payments = await _dbSet.Where(p => p.LoanId == loanId).ToListAsync();
            var totalPrincipal = payments.Sum(p => p.PrincipalPart);
            var paidPrincipal = payments.Where(p => p.Status == PaymentStatus.Paid).Sum(p => p.PrincipalPart);
            return totalPrincipal - paidPrincipal;
        }
    }

    public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
    {
        public AuditLogRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId)
        {
            return await _dbSet.Where(a => a.UserId == userId)
                               .OrderByDescending(a => a.Timestamp)
                               .Take(100)
                               .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityAsync(string entityType, Guid entityId)
        {
            return await _dbSet.Where(a => a.EntityType == entityType && a.EntityId == entityId)
                               .OrderByDescending(a => a.Timestamp)
                               .ToListAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetRecentLogsAsync(int count = 100)
        {
            return await _dbSet.OrderByDescending(a => a.Timestamp)
                               .Take(count)
                               .ToListAsync();
        }
    }

    public class UserRepository : GenericRepository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context) { }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByPhoneAsync(string phone)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Phone == phone);
        }

        public async Task<User?> GetWithLoansAsync(Guid userId)
        {
            return await _dbSet.Include(u => u.Loans)
                               .FirstOrDefaultAsync(u => u.UserId == userId);
        }
    }
}
