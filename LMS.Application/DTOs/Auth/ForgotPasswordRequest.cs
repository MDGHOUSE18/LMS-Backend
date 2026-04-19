using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.DTOs.Auth
{
    public class ForgotPasswordRequest
    {
        [Required]
        public string Token { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        [MinLength(8, ErrorMessage = "Password must be atleast 8 characters")]
        [MaxLength(50)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character."
        )]
        public string NewPassword { get; set; }
    }
}
