using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAppointmentSystem.DTOs;
using SmartAppointmentSystem.Data;
using SmartAppointmentSystem.Models;
using System.Text;
using System.Security.Claims;

namespace SmartAppointmentSystem.Controllers
{
    [ApiController]
    public class PatientController : Controller
    {
        private readonly ILogger<PatientController> _logger;
        private readonly SmartAppointmentSystemContext _context;
        public PatientController(ILogger<PatientController> logger, SmartAppointmentSystemContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpPost]
        [Route("api/[controller]/register")]
        public IActionResult RegisterPatient([FromBody] PatientRequestDTO patientRequest)
        {
            try
            {
                // Check if the email already exists
                var existingPatient = _context.Patients.FirstOrDefault(p => p.Email == patientRequest.Email);
                if (existingPatient != null)
                {
                    return BadRequest("Email already exists. Please use a different email.");
                }
                if (string.IsNullOrEmpty(patientRequest.Password))
                {
                    return BadRequest("Password cannot be null or empty.");
                }

                // Hash the password before saving
                var hashedPassword = HashPassword(patientRequest.Password);
               

                // Map DTO to model
                var patient = new Patient
                {
                    FirstName = patientRequest.FirstName,
                    LastName = patientRequest.LastName,
                    Email = patientRequest.Email,
                    Password = hashedPassword
                };

                _context.Patients.Add(patient);
                _context.SaveChanges();

                return Ok("Patient registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering the patient.");
                return StatusCode(500, "Internal server error");
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("api/[controller]/createBooking")]
        public async Task<IActionResult> createBooking(int DoctorId, string Description)
        {
            try
            {
                var emailClaim = HttpContext.User.FindFirst(ClaimTypes.Email);
                if (emailClaim == null)
                {
                    return Unauthorized("Email is missing from the token.");
                }

                string email = emailClaim.Value;

                // Find the patient record using the email
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.Email == email);
                if (patient == null)
                {
                    return NotFound("Patient record not found for the given email.");
                }
                var patientId = patient.Id;
                var booking = new Booking
                {   
                    PatientId = patientId,
                    DoctorId = DoctorId,
                    Description = Description,
                    Status = Status.pending,
                };
                _context.Bookings.Add(booking);
                _context.SaveChanges();
                return Ok("Booking successfully created");
            }
            catch (Exception ex)
            {
                // Log the error and return a server error response
                _logger.LogError(ex, "An error occurred while creating the booking.");
                return StatusCode(500, "Internal server error");
            }

        }

        [Authorize]
        [HttpGet()]
        [Route("api/[controller]/getBookings")]
        public async Task<IActionResult> getPatientBookings()
        {
            try
            {
                var patientEmail = HttpContext.User.FindFirst(ClaimTypes.Email);

                if(patientEmail == null)
                {
                    return NotFound("Patient's email does not exist");
                }
                string email = patientEmail.Value;

                var patient = await _context.Patients
              .Include(p => p.Bookings) // Load related bookings
              .FirstOrDefaultAsync(p => p.Email == email);

                if (patient == null)
                {
                    return NotFound("No patient record found for the given email.");
                }

                // Extract patient's bookings
                var bookings = patient.Bookings.Select(b => new
                {
                    b.BookingId,
                    b.DoctorId,
                    b.Description,
                    b.Status,
                  
                });

                // Return structured response
                return Ok(new
                {
                    PatientId = patient.Id,
                    PatientName = $"{patient.FirstName} {patient.LastName}",
                    Bookings = bookings
                });


            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the booking.");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpPost()]
        [Route("api/[controller]/createReview")]
        public async Task<IActionResult> createReviewOnBooking([FromBody] CreateReviewDTO request)
        {
            
            var review = new Review
            {
                Description = request.Description,
                Stars = request.Stars,
                BookingId = request.BookingId,
            };
             _context.Reviews.Add(review);
            _context.SaveChanges();
            return Ok("created review");
        }

  


    }
}
