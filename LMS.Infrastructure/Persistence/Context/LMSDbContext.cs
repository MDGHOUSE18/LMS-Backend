using LMS.Domain.Entities.Audit;
using LMS.Domain.Entities.Auth;
using LMS.Domain.Entities.Loan;
using LMS.Domain.Entities.Lookup;
using LMS.Domain.Entities.Workflow;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Context
{
    public class LMSDbContext : DbContext, IUnitOfWork
    {
        public LMSDbContext(DbContextOptions<LMSDbContext> options) : base(options) { }

        // ==================== DBO SCHEMA ====================
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<UserLoginHistory> UserLoginHistories { get; set; } = null!;
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; } = null!;
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; } = null!;
        public DbSet<OtpRequest> OtpRequests { get; set; } = null!;
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;

        // ==================== LOAN SCHEMA ====================
        public DbSet<LoanApplication> LoanApplications { get; set; } = null!;
        public DbSet<LoanFinancialDetails> LoanFinancialDetails { get; set; } = null!;
        public DbSet<Document> Documents { get; set; } = null!;
        public DbSet<LoanAssignment> LoanAssignments { get; set; } = null!;
        public DbSet<EligibilityResult> EligibilityResults { get; set; } = null!;

        // ==================== LOOKUP SCHEMA ====================
        public DbSet<LoanPurpose> LoanPurposes { get; set; } = null!;
        public DbSet<LoanStatus> LoanStatuses { get; set; } = null!;
        public DbSet<EmploymentType> EmploymentTypes { get; set; } = null!;
        public DbSet<DocumentType> DocumentTypes { get; set; } = null!;
        public DbSet<VerificationStatus> VerificationStatuses { get; set; } = null!;

        // ==================== WORKFLOW SCHEMA ====================
        public DbSet<LoanStatusHistory> LoanStatusHistories { get; set; } = null!;

        // ==================== AUDIT SCHEMA ====================
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserLoginHistory> UserLoginHistory { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<OtpRequest> OtpRequests { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanFinancialDetails> LoanFinancialDetails { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================================================
            // 1. EXPLICIT TABLE & SCHEMA MAPPINGS (DB-First Exact Match)
            // =========================================================
            modelBuilder.Entity<User>().ToTable("Users", "dbo");
            modelBuilder.Entity<Role>().ToTable("Roles", "dbo");
            modelBuilder.Entity<UserLoginHistory>().ToTable("UserLoginHistory", "dbo");
            modelBuilder.Entity<UserRefreshToken>().ToTable("UserRefreshTokens", "dbo");
            modelBuilder.Entity<EmailVerificationToken>().ToTable("EmailVerificationTokens", "dbo");
            modelBuilder.Entity<OtpRequest>().ToTable("OTPRequests", "dbo");
            modelBuilder.Entity<PasswordResetToken>().ToTable("PasswordResetTokens", "dbo");

            modelBuilder.Entity<LoanApplication>().ToTable("LoanApplication", "loan");
            modelBuilder.Entity<LoanFinancialDetails>().ToTable("LoanFinancialDetails", "loan");
            modelBuilder.Entity<Document>().ToTable("Documents", "loan");
            modelBuilder.Entity<LoanAssignment>().ToTable("LoanAssignment", "loan");
            modelBuilder.Entity<EligibilityResult>().ToTable("EligibilityResults", "loan");

            modelBuilder.Entity<LoanPurpose>().ToTable("LoanPurpose", "lookup");
            modelBuilder.Entity<LoanStatus>().ToTable("LoanStatus", "lookup");
            modelBuilder.Entity<EmploymentType>().ToTable("EmploymentType", "lookup");
            modelBuilder.Entity<DocumentType>().ToTable("DocumentType", "lookup");
            modelBuilder.Entity<VerificationStatus>().ToTable("VerificationStatus", "lookup");

            modelBuilder.Entity<LoanStatusHistory>().ToTable("LoanStatusHistory", "workflow");
            modelBuilder.Entity<AuditLog>().ToTable("AuditLogs", "audit");

            // =========================================================
            // 2. RELATIONSHIPS & FOREIGN KEYS (Exact SQL Constraint Match)
            // =========================================================

            // --- USER & AUTH RELATIONSHIPS ---
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict); // DB: NO ACTION

            modelBuilder.Entity<EmailVerificationToken>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade); // DB: ON DELETE CASCADE

            modelBuilder.Entity<OtpRequest>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PasswordResetToken>()
                .HasOne(p => p.User)
                .WithMany()
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserLoginHistory>()
                .HasOne(h => h.User)
                .WithMany()
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserRefreshToken>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // --- LOAN CORE RELATIONSHIPS ---
            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.Status)
                .WithMany()
                .HasForeignKey(l => l.StatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.Purpose)
                .WithMany()
                .HasForeignKey(l => l.PurposeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.EmploymentType)
                .WithMany()
                .HasForeignKey(l => l.EmploymentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanFinancialDetails>()
                .HasOne(f => f.LoanApplication)
                .WithOne() // 1:1 relationship
                .HasForeignKey<LoanFinancialDetails>(f => f.LoanApplicationId)
                .OnDelete(DeleteBehavior.Cascade); // DB: ON DELETE CASCADE

            modelBuilder.Entity<LoanAssignment>()
                .HasOne(a => a.LoanApplication)
                .WithMany()
                .HasForeignKey(a => a.LoanApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LoanAssignment>()
                .HasOne(a => a.AssignedOfficer)
                .WithMany()
                .HasForeignKey(a => a.AssignedOfficerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.LoanApplication)
                .WithMany()
                .HasForeignKey(d => d.LoanApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.DocumentType)
                .WithMany(dt => dt.Documents)
                .HasForeignKey(d => d.DocumentTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.VerificationStatus)
                .WithMany()
                .HasForeignKey(d => d.VerificationStatusId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.UploadedByUser)
                .WithMany()
                .HasForeignKey(d => d.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<EligibilityResult>()
                .HasOne(e => e.LoanApplication)
                .WithMany()
                .HasForeignKey(e => e.LoanApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- WORKFLOW RELATIONSHIPS ---
            modelBuilder.Entity<LoanStatusHistory>()
                .HasOne(h => h.LoanApplication)
                .WithMany()
                .HasForeignKey(h => h.LoanApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // LoanApplication → User relationship
            modelBuilder.Entity<LoanApplication>()
                .HasOne(la => la.User)
                .WithMany(u => u.LoanApplications)
                .HasForeignKey(la => la.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // LoanFinancialDetails → LoanApplication relationship
            modelBuilder.Entity<LoanFinancialDetails>()
                .HasOne(lfd => lfd.LoanApplication)
                .WithOne(la => la.FinancialDetails)
                .HasForeignKey<LoanFinancialDetails>(lfd => lfd.LoanApplicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Document → LoanApplication relationship
            modelBuilder.Entity<Document>()
                .HasOne(d => d.LoanApplication)
                .WithMany(la => la.Documents)
                .HasForeignKey(d => d.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            // Document → User (uploaded by) relationship
            modelBuilder.Entity<Document>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Payment → LoanApplication relationship
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.LoanApplication)
                .WithMany(la => la.Payments)
                .HasForeignKey(p => p.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            // AuditLog → User relationship
            modelBuilder.Entity<AuditLog>()
                .HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }
    }
}
