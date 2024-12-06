using SmartAppointmentSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace SmartAppointmentSystem.DTOs
{
    public class DoctorRequestDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Price must be a positive value.")]
        public int Price { get; set; }

        [Required]
        public Category Category { get; set; }
    }
}
