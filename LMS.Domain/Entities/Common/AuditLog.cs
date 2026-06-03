using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Common
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        
        public string Action { get; set; } // Create, Update, Approve, Reject, Delete
        public string EntityType { get; set; } // Loan, Document, User
        public Guid EntityId { get; set; }
        
        public DateTime Timestamp { get; set; }
        public string? Details { get; set; } // JSON details
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? CorrelationId { get; set; }
        
        // Navigation properties
        public virtual User? User { get; set; }
    }
}
