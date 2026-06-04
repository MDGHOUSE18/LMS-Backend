using LMS.Domain.Entities.Audit;
using LMS.Domain.Entities.Auth;
using LMS.Domain.Entities.Loan;
using LMS.Domain.Entities.Lookup;
using LMS.Domain.Entities.Workflow;
using AuditLog = LMS.Domain.Entities.Audit.AuditLog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LMS.Infrastructure.Persistence.Context
{
    public class LMSDbContext : DbContext, IUnitOfWork
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;
        private string? _currentUserId;
        private string? _currentUserIpAddress;

        public LMSDbContext(DbContextOptions<LMSDbContext> options) 
            : this(options, null, null, null) { }

        public LMSDbContext(
            DbContextOptions<LMSDbContext> options,
            IHttpContextAccessor? httpContextAccessor = null,
            string? currentUserId = null,
            string? currentUserIpAddress = null) 
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _currentUserId = currentUserId;
            _currentUserIpAddress = currentUserIpAddress;

            // Try to extract user info from HttpContext if available
            if (_httpContextAccessor?.HttpContext?.User != null)
            {
                var claimsPrincipal = _httpContextAccessor.HttpContext.User;
                if (string.IsNullOrEmpty(_currentUserId))
                {
                    _currentUserId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                        ?? claimsPrincipal.FindFirst("userId")?.Value;
                }
                if (string.IsNullOrEmpty(_currentUserIpAddress))
                {
                    _currentUserIpAddress = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
                }
            }
        }

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

        public DbSet<Payment> Payments { get; set; } = null!;

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
                .HasOne(d => d.Loan)
                .WithMany(la => la.Documents)
                .HasForeignKey(d => d.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Loan)
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

        /// <summary>
        /// Overrides SaveChangesAsync to automatically capture audit logs for all entity changes.
        /// Complies with RBI's 10-year retention requirement by logging Create, Update, and Delete operations.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditLogs = new List<AuditLog>();
            var currentUserId = GetCurrentUserId();
            var ipAddress = GetCurrentUserIpAddress();
            var timestamp = DateTime.UtcNow;

            // Detect all changes using ChangeTracker
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLog && 
                           (e.State == EntityState.Added || 
                            e.State == EntityState.Modified || 
                            e.State == EntityState.Deleted))
                .ToList();

            foreach (var entry in entries)
            {
                var entityType = entry.Entity.GetType().Name;
                var entityId = GetEntityId(entry.Entity);
                
                if (string.IsNullOrEmpty(entityId))
                {
                    continue; // Skip entities without a valid ID
                }

                var auditLog = new AuditLog
                {
                    UserId = currentUserId ?? "SYSTEM",
                    Action = entry.State.ToString(),
                    EntityType = entityType,
                    EntityId = entityId,
                    Timestamp = timestamp,
                    IpAddress = ipAddress,
                    Details = SerializeAuditDetails(entry)
                };

                auditLogs.Add(auditLog);
            }

            // Add all audit logs before saving changes
            if (auditLogs.Any())
            {
                await AuditLogs.AddRangeAsync(auditLogs, cancellationToken);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Gets the current user ID from HttpContext, constructor parameter, or defaults to SYSTEM.
        /// </summary>
        private string? GetCurrentUserId()
        {
            if (!string.IsNullOrEmpty(_currentUserId))
            {
                return _currentUserId;
            }

            if (_httpContextAccessor?.HttpContext?.User != null)
            {
                var claimsPrincipal = _httpContextAccessor.HttpContext.User;
                return claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? claimsPrincipal.FindFirst("userId")?.Value;
            }

            return null;
        }

        /// <summary>
        /// Gets the current user's IP address from HttpContext or constructor parameter.
        /// </summary>
        private string? GetCurrentUserIpAddress()
        {
            if (!string.IsNullOrEmpty(_currentUserIpAddress))
            {
                return _currentUserIpAddress;
            }

            return _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        /// <summary>
        /// Extracts the primary key value from an entity as a string.
        /// </summary>
        private string GetEntityId(object entity)
        {
            var keyProperties = Entry(entity).Metadata.FindPrimaryKey()?.Properties;
            
            if (keyProperties == null || !keyProperties.Any())
            {
                return string.Empty;
            }

            var keyValues = keyProperties
                .Select(p => Entry(entity).Property(p.Name).CurrentValue)
                .Where(v => v != null)
                .Select(v => v!.ToString());

            return string.Join("-", keyValues);
        }

        /// <summary>
        /// Serializes entity changes into a JSON payload for audit storage.
        /// Includes BeforeData (original values) and AfterData (new/modified values).
        /// </summary>
        private string SerializeAuditDetails(EntityEntry entry)
        {
            var details = new
            {
                BeforeData = entry.State == EntityState.Modified || entry.State == EntityState.Deleted
                    ? CaptureOriginalValues(entry)
                    : null,
                AfterData = entry.State == EntityState.Added || entry.State == EntityState.Modified
                    ? CaptureCurrentValues(entry)
                    : null,
                ModifiedProperties = entry.State == EntityState.Modified
                    ? entry.Properties
                        .Where(p => p.IsModified)
                        .Select(p => p.Metadata.Name)
                        .ToArray()
                    : null
            };

            return JsonSerializer.Serialize(details, new JsonSerializerOptions
            {
                WriteIndented = false,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            });
        }

        /// <summary>
        /// Captures original property values before modification/deletion.
        /// </summary>
        private Dictionary<string, object?> CaptureOriginalValues(EntityEntry entry)
        {
            return entry.Properties
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => p.OriginalValue);
        }

        /// <summary>
        /// Captures current property values after addition/modification.
        /// </summary>
        private Dictionary<string, object?> CaptureCurrentValues(EntityEntry entry)
        {
            return entry.Properties
                .ToDictionary(
                    p => p.Metadata.Name,
                    p => p.CurrentValue);
        }
    }
}
