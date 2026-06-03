using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Domain.Entities.Lookup
{
    public class VerificationStatus
    {
        public int Id { get; set; }
        public string StatusName { get; set; } = string.Empty;

        // Navigation Properties
        public ICollection<Document> Documents { get; set; } = new List<Document>();
    }
}
