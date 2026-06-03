using LMS.Domain.Entities.Auth;
using LMS.Domain.Entities.Loan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Lookup
{
    public class DocumentType
    {
        public int Id { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public bool IsMandatory { get; set; }
        public string? AllowedFileTypes { get; set; }
        public int? MaxFileSizeMB { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation Properties
        public User? CreatedByUser { get; set; }
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
