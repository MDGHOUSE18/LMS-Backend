using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.DTOs.Auth
{
    public class RegisterRequestDto
    {
        [Required]
        [MinLength(3,ErrorMessage = "Name must be atleast 3 characters")]
        [MaxLength(256)]
        public string? FullName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email Format is invalid")]
        [MaxLength(256)]
        public string? Email { get; set; }
        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
        public string MobileNumber { get; set; }
        [Required(ErrorMessage = "Password is Required")]
        [MinLength(8, ErrorMessage = "Password must be atleast 8 characters")]
        [MaxLength(50)]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z0-9]).{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, number, and special character."
        )]
        public string? Password { get; set; }

    }
}

