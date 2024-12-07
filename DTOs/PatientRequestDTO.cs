using System.ComponentModel.DataAnnotations;

namespace SmartAppointmentSystem.DTOs
{
    public class PatientRequestDTO
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public string Password { get; set; } // Plain-text for now; hash it before saving to DB
    }

    public class CreateReviewDTO
    {

        public string Description { get; set; }

        public int Stars { get; set; }

        [Required]
        public int BookingId { get; set; }
    }
}
