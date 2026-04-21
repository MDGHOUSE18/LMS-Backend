using LMS.Domain.Entities.Auth;
using LMS.Domain.Entities.Loan;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Context
{
    public class LMSDbContext : DbContext
    {
        public LMSDbContext(DbContextOptions<LMSDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserLoginHistory> UserLoginHistory { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<OtpRequest> OtpRequests { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<LoanFinancialDetails> LoanFinancialDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // ✅ FOREIGN KEY RELATIONSHIPS

            // User → Role relationship
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany()
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // OtpRequest → User relationship
            modelBuilder.Entity<OtpRequest>()
                .HasOne(o => o.User)           
                .WithMany(u => u.OtpRequests)  
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRefreshToken>()
                .HasOne<User>()
                .WithMany(u => u.UserRefreshTokens)   
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserLoginHistory>()
                .HasOne<User>()
                .WithMany(u => u.UserLoginHistories)  
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // PasswordResetToken → User relationship
            modelBuilder.Entity<PasswordResetToken>()
                .HasOne<User>()
                .WithMany(u => u.PasswordResetTokens)
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
