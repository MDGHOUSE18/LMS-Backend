using LMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LMS.Application.DTOs.Auth
{
    public class VerifyOtpRequestDto
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email Format is invalid")]
        [MaxLength(256)]
        public string? Email { get; set; }
        [Required]
        [Length(6,6)]
        public string? Otp { get; set; }
    }
}
