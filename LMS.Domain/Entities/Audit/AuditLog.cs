using LMS.Domain.Entities.Auth;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LMS.Domain.Entities.Audit
{
    /// <summary>
    /// AuditLog entity for compliance with RBI's 10-year retention requirement.
    /// Tracks all Create, Update, and Delete operations on entities.
    /// </summary>
    public class AuditLog
    {
        [Key]
        public Guid LogId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        public string EntityId { get; set; } = string.Empty;

        [Required]
        public DateTime Timestamp { get; set; }

        [Required]
        [Column(TypeName = "nvarchar(max)")]
        public string Details { get; set; } = string.Empty; // JSON payload containing before/after data

        [MaxLength(45)] // IPv6 max length
        public string? IpAddress { get; set; }

        /// <summary>
        /// Optional: Retention expiry date (10 years from timestamp for RBI compliance)
        /// </summary>
        public DateTime RetentionExpiryDate => Timestamp.AddYears(10);

        // Navigation Properties
        public User User { get; set; } = null!;
    }
}
