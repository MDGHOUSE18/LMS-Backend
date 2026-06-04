using LMS.Application.Interfaces.Services.Loan;
using LMS.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace LMS.Application.Services.Loan
{
    public class AuditService : IAuditService
    {
        private readonly string? _ipAddress;
        private readonly string? _userAgent;
        private readonly string? _correlationId;

        public AuditService(string? ipAddress = null, string? userAgent = null, string? correlationId = null)
        {
            _ipAddress = ipAddress;
            _userAgent = userAgent;
            _correlationId = correlationId;
        }

        public async Task LogAsync(string entity, Guid entityId, string action, object? before, object? after)
        {
            await LogAsync(entity, entityId, action, before, after, null);
        }

        /// <summary>
        /// Log audit trail entry for compliance (10-year retention per RBI)
        /// </summary>
        public async Task<AuditLog> LogAsync(
            string entityType, 
            Guid entityId, 
            string action, 
            object? before, 
            object? after,
            Guid? userId = null)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                Details = SerializeDetails(before, after),
                IpAddress = _ipAddress,
                UserAgent = _userAgent,
                CorrelationId = _correlationId ?? Guid.NewGuid().ToString()
            };

            // In production, this would save to database
            // For now, we return the audit log object
            await Task.CompletedTask;
            
            return auditLog;
        }

        /// <summary>
        /// Log with current user context automatically captured
        /// </summary>
        public async Task<AuditLog> LogWithUserAsync(
            string entityType,
            Guid entityId,
            string action,
            object? before,
            object? after,
            Guid userId,
            string ipAddress,
            string? userAgent = null)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                EntityType = entityType,
                EntityId = entityId,
                Action = action,
                Timestamp = DateTime.UtcNow,
                Details = SerializeDetails(before, after),
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CorrelationId = Guid.NewGuid().ToString()
            };

            await Task.CompletedTask;
            return auditLog;
        }

        private string? SerializeDetails(object? before, object? after)
        {
            if (before == null && after == null)
                return null;

            var details = new
            {
                Before = before,
                After = after,
                ChangedAt = DateTime.UtcNow
            };

            try
            {
                return JsonSerializer.Serialize(details, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
            }
            catch
            {
                // Fallback for objects that can't be serialized
                return $"{{\"Action\": \"Logged at {DateTime.UtcNow}\"}}";
            }
        }

        /// <summary>
        /// Create audit log for loan status change
        /// </summary>
        public async Task<AuditLog> LogLoanStatusChangeAsync(
            Guid loanId,
            string oldStatus,
            string newStatus,
            Guid changedByUserId,
            string? reason = null)
        {
            var details = new
            {
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Reason = reason,
                ChangedAt = DateTime.UtcNow
            };

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = changedByUserId,
                EntityType = "Loan",
                EntityId = loanId,
                Action = $"STATUS_CHANGE: {oldStatus} → {newStatus}",
                Timestamp = DateTime.UtcNow,
                Details = JsonSerializer.Serialize(details),
                IpAddress = _ipAddress,
                CorrelationId = _correlationId ?? Guid.NewGuid().ToString()
            };

            await Task.CompletedTask;
            return auditLog;
        }

        /// <summary>
        /// Create audit log for document verification
        /// </summary>
        public async Task<AuditLog> LogDocumentVerificationAsync(
            Guid documentId,
            Guid loanId,
            string documentType,
            string oldStatus,
            string newStatus,
            Guid verifiedByUserId,
            string? rejectionReason = null)
        {
            var details = new
            {
                DocumentType = documentType,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                RejectionReason = rejectionReason,
                VerifiedAt = DateTime.UtcNow
            };

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = verifiedByUserId,
                EntityType = "Document",
                EntityId = documentId,
                Action = $"DOCUMENT_VERIFICATION: {oldStatus} → {newStatus}",
                Timestamp = DateTime.UtcNow,
                Details = JsonSerializer.Serialize(details),
                IpAddress = _ipAddress,
                CorrelationId = _correlationId ?? Guid.NewGuid().ToString()
            };

            await Task.CompletedTask;
            return auditLog;
        }
    }
}
