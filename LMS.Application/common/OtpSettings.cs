using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.common
{
    public class OtpSettings
    {
        public string SecretKey { get; set; }
        public bool EnableBypass { get; set; }
        public string BypassCode { get; set; }
    }
}
