using LMS.Domain.Entities.Audit;
using LMS.Domain.Entities.Auth;
using LMS.Domain.Entities.Loan;
using LMS.Domain.Entities.Lookup;
using LMS.Domain.Entities.Workflow;
using LMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS.Infrastructure.Persistence.Configurations
{
    /// <summary>
    /// EF mappings aligned with the DB-first SQL script (schemas: dbo, loan, lookup, workflow, audit).
    /// </summary>
    internal static class LmsDbFirstConfiguration
    {
        public static void Apply(ModelBuilder modelBuilder)
        {
            ConfigureAuth(modelBuilder);
            ConfigureLookups(modelBuilder);
            ConfigureLoan(modelBuilder);
            ConfigureWorkflow(modelBuilder);
            ConfigureAudit(modelBuilder);
        }

        private static void ConfigureAuth(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users", "dbo");
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id).HasColumnName("UserId")
                    .HasConversion(v => v.ToString(), v => Guid.Parse(v));
                entity.Property(u => u.Mobile).HasColumnName("Phone");
                entity.Property(u => u.DateOfBirth).HasColumnName("Dob");
                entity.Property(u => u.IsEmailVerified).HasColumnName("IsVerified");
                entity.Property(u => u.LastModifiedAt).HasColumnName("UpdatedAt");

                entity.Ignore(u => u.IsActive);
                entity.Ignore(u => u.FailedLoginAttempts);
                entity.Ignore(u => u.IsLocked);
                entity.Ignore(u => u.LockoutEnd);
                entity.Ignore(u => u.IsMobileVerified);
                entity.Ignore(u => u.MobileVerifiedAt);
                entity.Ignore(u => u.EmailVerifiedAt);
                entity.Ignore(u => u.PanNumber);
                entity.Ignore(u => u.OtpRequests);
                entity.Ignore(u => u.UserRefreshTokens);
                entity.Ignore(u => u.UserLoginHistories);
                entity.Ignore(u => u.PasswordResetTokens);
                entity.Ignore(u => u.Loans);
                entity.Ignore(u => u.Documents);
                entity.Ignore(u => u.AuditLogs);

                entity.HasOne(u => u.Role)
                    .WithMany()
                    .HasForeignKey(u => u.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("Roles", "dbo");
                entity.Property(r => r.Id).HasColumnName("RoleId");
                entity.Property(r => r.RoleName).HasColumnName("RoleName");
            });

            modelBuilder.Entity<EmailVerificationToken>(entity =>
            {
                entity.ToTable("EmailVerificationTokens", "dbo");
                entity.Property(e => e.Id).HasColumnName("EmailVerificationTokenId");
                entity.Property(e => e.UserId).HasColumnName("UserId")
                    .HasConversion(v => v.ToString(), v => Guid.Parse(v));
                entity.Property(e => e.TokenHash).HasColumnName("Token");
                entity.Property(e => e.ExpiresAt).HasColumnName("ExpiryDate");
                entity.Ignore(e => e.CreatedAt);
                entity.Ignore(e => e.VerifiedAt);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserLoginHistory>(entity =>
            {
                entity.ToTable("UserLoginHistory", "dbo");
                entity.Property(h => h.Id).HasColumnName("UserLoginHistoryId");
                entity.Property(h => h.UserId).HasColumnName("UserId")
                    .HasConversion(v => v.ToString(), v => Guid.Parse(v));
                entity.Property(h => h.LoginTime).HasColumnName("LoginTime");
                entity.Ignore(h => h.LogoutTime);
                entity.Ignore(h => h.IsSuccess);
                entity.Ignore(h => h.FailureReason);
                entity.Ignore(h => h.User);
                entity.HasOne<User>().WithMany().HasForeignKey(h => h.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<UserRefreshToken>(entity =>
            {
                entity.ToTable("UserRefreshTokens", "dbo");
                entity.Property(t => t.Id).HasColumnName("UserRefreshTokenId");
                entity.Property(t => t.UserId).HasColumnName("UserId")
                    .HasConversion(v => v.ToString(), v => Guid.Parse(v));
                entity.Property(t => t.TokenHash).HasColumnName("Token");
                entity.Property(t => t.ExpiryDate).HasColumnName("ExpiryDate");
                entity.Ignore(t => t.IsRevoked);
                entity.Ignore(t => t.CreatedAt);
                entity.Ignore(t => t.RevokedAt);
                entity.Ignore(t => t.ReplacedByTokenHash);
                entity.Ignore(t => t.IpAddress);
                entity.Ignore(t => t.IsActive);
                entity.Ignore(t => t.User);
                entity.HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OtpRequest>(entity =>
            {
                entity.ToTable("OTPRequests", "dbo");
                entity.Property(o => o.Id).HasColumnName("OtpRequestId");
                entity.Property(o => o.UserId).HasColumnName("UserId")
                    .HasConversion(v => v.ToString(), v => Guid.Parse(v));
                entity.Property(o => o.OTPHash).HasColumnName("OtpHash");
                entity.Property(o => o.ExpiresAt).HasColumnName("ExpiryDate");
                entity.Ignore(o => o.Purpose);
                entity.Ignore(o => o.MobileNumber);
                entity.Ignore(o => o.AttemptCount);
                entity.Ignore(o => o.MaxAttempts);
                entity.Ignore(o => o.Isused);
                entity.Ignore(o => o.CreatedAt);
                entity.Ignore(o => o.VerifiedAt);
                entity.Ignore(o => o.User);
                entity.HasOne<User>().WithMany().HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<PasswordResetToken>(entity =>
            {
                entity.ToTable("PasswordResetTokens", "dbo");
                entity.Property(p => p.Id).HasColumnName("PasswordResetTokenId");
                entity.Property(p => p.UserId).HasColumnName("UserId")
                    .HasConversion(v => v.ToString(), v => Guid.Parse(v));
                entity.Property(p => p.TokenHash).HasColumnName("Token");
                entity.Property(p => p.ExpiresAt).HasColumnName("ExpiryDate");
                entity.Ignore(p => p.IsUsed);
                entity.Ignore(p => p.CreatedAt);
                entity.Ignore(p => p.User);
                entity.HasOne<User>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureLookups(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoanStatus>(entity =>
            {
                entity.ToTable("LoanStatus", "lookup");
                entity.Property(s => s.Id).HasColumnName("LoanStatusId");
                entity.Ignore(s => s.LoanApplications);
            });

            modelBuilder.Entity<LoanPurpose>(entity =>
            {
                entity.ToTable("LoanPurpose", "lookup");
                entity.Property(p => p.Id).HasColumnName("LoanPurposeId");
                entity.Ignore(p => p.LoanApplications);
            });

            modelBuilder.Entity<EmploymentType>(entity =>
            {
                entity.ToTable("EmploymentType", "lookup");
                entity.Property(e => e.Id).HasColumnName("EmploymentTypeId");
                entity.Ignore(e => e.LoanApplications);
            });

            modelBuilder.Entity<DocumentType>(entity =>
            {
                entity.ToTable("DocumentType", "lookup");
                entity.Property(d => d.Id).HasColumnName("DocumentTypeId");
                entity.Property(d => d.MaxFileSizeMB).HasColumnName("MaxSizeMb");
                entity.Property(d => d.AllowedFileTypes).HasColumnName("AllowedExtensions");
                entity.Ignore(d => d.IsMandatory);
                entity.Ignore(d => d.CreatedBy);
                entity.Ignore(d => d.CreatedAt);
                entity.Ignore(d => d.CreatedByUser);
                entity.Ignore(d => d.Documents);
            });

            modelBuilder.Entity<VerificationStatus>(entity =>
            {
                entity.ToTable("VerificationStatus", "lookup");
                entity.Property(v => v.Id).HasColumnName("VerificationStatusId");
                entity.Ignore(v => v.Documents);
            });
        }

        private static void ConfigureLoan(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoanApplication>(entity =>
            {
                entity.ToTable("LoanApplication", "loan");
                entity.HasKey(l => l.Id);
                entity.Property(l => l.Id).HasColumnName("LoanApplicationId");
                entity.Property(l => l.UserId).HasColumnName("UserId")
                    .HasConversion(v => v.ToString(), v => Guid.Parse(v));
                entity.Property(l => l.Status).HasColumnName("LoanStatusId")
                    .HasConversion(s => (int)s, i => (LoanStatusEnum)i);
                entity.Property<int?>("LoanPurposeId").HasColumnName("LoanPurposeId");
                entity.Property<int?>("EmploymentTypeId").HasColumnName("EmploymentTypeId");

                entity.Ignore(l => l.Purpose);
                entity.Ignore(l => l.EmploymentType);
                entity.Ignore(l => l.RejectedDate);
                entity.Ignore(l => l.LastModifiedAt);
                entity.Ignore(l => l.CreatedBy);
                entity.Ignore(l => l.LastModifiedBy);
                entity.Ignore(l => l.ApprovedBy);
                entity.Ignore(l => l.RejectedBy);
                entity.Ignore(l => l.LoanAmount);
                entity.Ignore(l => l.TenureMonths);
                entity.Ignore(l => l.InterestRate);
                entity.Ignore(l => l.MonthlyIncome);
                entity.Ignore(l => l.ExistingEMI);
                entity.Ignore(l => l.CalculatedEMI);
                entity.Ignore(l => l.TotalInterestPayable);
                entity.Ignore(l => l.TotalPayableAmount);
                entity.Ignore(l => l.User);
                entity.Ignore(l => l.FinancialDetails);
                entity.Ignore(l => l.Documents);
                entity.Ignore(l => l.Payments);
                entity.Ignore(l => l.StatusHistories);
                entity.Ignore(l => l.AmortizationSchedules);
            });

            modelBuilder.Entity<LoanFinancialDetails>(entity =>
            {
                entity.ToTable("LoanFinancialDetails", "loan");
                entity.Property(f => f.Id).HasColumnName("LoanFinancialDetailsId");
                entity.Property(f => f.LoanApplicationId).HasColumnName("LoanApplicationId");
                entity.Property(f => f.LoanAmount).HasColumnName("Amount");
                entity.Property(f => f.InterestRate).HasColumnName("AnnualInterest");
                entity.Property(f => f.ExistingEMI).HasColumnName("ExistingEmi");
                entity.Property(f => f.Emi).HasColumnName("Emi");
                entity.Ignore(f => f.LoanApplication);
            });

            modelBuilder.Entity<Document>(entity =>
            {
                entity.ToTable("Documents", "loan");
                entity.Property(d => d.Id).HasColumnName("DocumentId");
                entity.Property(d => d.LoanId).HasColumnName("LoanId");
                entity.Property(d => d.UserId).HasColumnName("UserId")
                    .HasConversion(v => v.ToString(), v => Guid.Parse(v));
                entity.Property(d => d.DocumentTypeId).HasColumnName("DocumentTypeId");
                entity.Property(d => d.VerificationStatus).HasColumnName("VerificationStatusId");
                entity.Property(d => d.FilePath).HasColumnName("S3Path");
                entity.Property(d => d.VerifiedBy).HasColumnName("VerifiedBy")
                    .HasConversion(
                        v => v == null ? null : v.ToString(),
                        v => v == null ? (Guid?)null : Guid.Parse(v));
                entity.Ignore(d => d.Type);
                entity.Ignore(d => d.FileName);
                entity.Ignore(d => d.FileSize);
                entity.Ignore(d => d.ContentType);
                entity.Ignore(d => d.ExpiryDate);
                entity.Ignore(d => d.Loan);
                entity.Ignore(d => d.User);
                entity.Ignore(d => d.Verifier);
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("Payments", "dbo");
                entity.Property(p => p.Id).HasColumnName("PaymentId");
                entity.Property(p => p.LoanId).HasColumnName("LoanId");
                entity.Ignore(p => p.PenaltyAmount);
                entity.Ignore(p => p.LastReminderSent);
                entity.Ignore(p => p.Loan);
            });

            modelBuilder.Entity<EligibilityResult>(entity =>
            {
                entity.ToTable("EligibilityResults", "loan");
                entity.Property(e => e.Id).HasColumnName("EligibilityResultId");
                entity.Property(e => e.LoanApplicationId).HasColumnName("LoanApplicationId");
                entity.Property(e => e.DTI).HasColumnName("DebtToIncomeRatio");
                entity.Property(e => e.EvaluatedAt).HasColumnName("CheckedAt");
                entity.Ignore(e => e.Age);
                entity.Ignore(e => e.MonthlyIncome);
                entity.Ignore(e => e.LoanApplication);
            });

            modelBuilder.Entity<LoanAssignment>(entity =>
            {
                entity.ToTable("LoanAssignment", "loan");
                entity.Property(a => a.Id).HasColumnName("LoanAssignmentId");
                entity.Property(a => a.LoanApplicationId).HasColumnName("LoanApplicationId");
                entity.Property(a => a.AssignedOfficerId).HasColumnName("AssignedOfficerId")
                    .HasConversion(v => v.ToString(), v => Guid.Parse(v));
                entity.Ignore(a => a.LoanApplication);
                entity.Ignore(a => a.AssignedOfficer);
            });
        }

        private static void ConfigureWorkflow(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoanStatusHistory>(entity =>
            {
                entity.ToTable("LoanStatusHistory", "workflow");
                entity.Property(h => h.Id).HasColumnName("LoanStatusHistoryId");
                entity.Property(h => h.LoanApplicationId).HasColumnName("LoanApplicationId");
                entity.Property(h => h.ChangedAt).HasColumnName("ChangedAt");
                entity.Property(h => h.ChangedBy).HasColumnName("ChangedBy")
                    .HasConversion(v => v.ToString(), v => Guid.Parse(v));

                entity.HasOne(h => h.FromStatus)
                    .WithMany(s => s.FromStatusHistories)
                    .HasForeignKey(h => h.FromStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.ToStatus)
                    .WithMany(s => s.ToStatusHistories)
                    .HasForeignKey(h => h.ToStatusId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.Ignore(h => h.LoanApplication);
                entity.Ignore(h => h.ChangedByUser);
            });
        }

        private static void ConfigureAudit(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.ToTable("AuditLogs", "audit");
                entity.Property(a => a.LogId).HasColumnName("AuditLogId");
                entity.Property(a => a.Action).HasMaxLength(50);
                entity.Ignore(a => a.User);
            });
        }
    }
}
