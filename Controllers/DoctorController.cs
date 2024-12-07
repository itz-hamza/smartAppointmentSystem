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
    public class DoctorController : ControllerBase
    {

        private readonly ILogger<DoctorController> _logger;
        private readonly SmartAppointmentSystemContext _context;

        public DoctorController(ILogger<DoctorController> logger, SmartAppointmentSystemContext context)
        {
            _logger = logger;
            _context = context;
        }

        [Authorize]
        [HttpGet]
        [Route("api/[controller]/getall")]
        public async Task<IActionResult> GetAllAsync()
        {
            var doctors = await _context.Doctors.
                            ToListAsync();

            return Ok(doctors);
        }


        [HttpPost]
        [Route("api/[controller]/register")]
        public IActionResult Add([FromBody] DoctorRequestDTO doctorRequest)
        {
            try
            {
                // Check if the email already exists
                var existingDoctor = _context.Doctors.FirstOrDefault(d => d.Email == doctorRequest.Email);
                if (existingDoctor != null)
                {
                    return BadRequest("Email already exists. Please use a different email.");
                }

                // Hash the password before saving
                var hashedPassword = HashPassword(doctorRequest.Password);

                
                var record = new Doctor
                {
                    Email = doctorRequest.Email,
                    Password = hashedPassword, // Save the hashed password
                    FirstName = doctorRequest.FirstName,
                    LastName = doctorRequest.LastName,
                    Price = doctorRequest.Price,
                    Category = doctorRequest.Category
                };

                _context.Doctors.Add(record);
                _context.SaveChanges();
                return Ok("Doctor registered successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while registering the doctor.");
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
        [HttpGet]
        [Route("api/Booking/getBookingsWithStatus")]
        public async Task<IActionResult> GetBookingsForDoctor(Status? status = null)
        {
            try
            {
                var DoctorEmail = HttpContext.User.FindFirst(ClaimTypes.Email);

                if (DoctorEmail == null)
                {
                    return NotFound("Doctor's email does not exist");
                }
                string email = DoctorEmail.Value;

                var doctor = await _context.Doctors
              .FirstOrDefaultAsync(p => p.Email == email);

                if (doctor == null)
                {
                    return NotFound("No doctor record found for the given email.");
                }

                var doctorId = doctor.Id;

                // Fetch bookings with filtering by doctor ID and optional status
                var query = _context.Bookings
                    .Include(b => b.Doctor)
                    .Include(b => b.Patient)
                    .Where(b => b.DoctorId == doctorId);

                // Apply optional status filter
                if (status.HasValue)
                {
                    query = query.Where(b => b.Status == status);
                }

                var bookings = await query
                    .Select(b => new
                    {
                        BookingId = b.BookingId,
                        Description = b.Description,
                        BookingStatus = b.Status.ToString(),
                        DoctorName = $"{b.Doctor.FirstName} {b.Doctor.LastName}",
                        PatientName = $"{b.Patient.FirstName} {b.Patient.LastName}"
                    })
                    .ToListAsync();

                if (bookings == null || bookings.Count == 0)
                {
                    return NotFound("No bookings found for this doctor.");
                }

                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching bookings.");
                return StatusCode(500, "Internal server error");
            }
        }
        [Authorize]
        [HttpGet]
        [Route("api/[controller]/getBookings")]
        public async Task<IActionResult> GetDetailedBookingsWithoutReviews()
        {
            try
            {
                var DoctorEmail = HttpContext.User.FindFirst(ClaimTypes.Email);

                if (DoctorEmail == null)
                {
                    return NotFound("Doctor's email does not exist");
                }
                string email = DoctorEmail.Value;

                var doctor = await _context.Doctors
              .FirstOrDefaultAsync(p => p.Email == email);

                if (doctor == null)
                {
                    return NotFound("No doctor record found for the given email.");
                }

                var doctorId = doctor.Id;

                var result = await _context.Bookings
                    .Where(b => b.DoctorId == doctorId)
                    .Select(b => new
                    {
                        BookingId = b.BookingId,
                        DoctorName = b.Doctor.FirstName + " " + b.Doctor.LastName,
                        DoctorPrice = b.Doctor.Price,
                        PatientName = b.Patient.FirstName + " " + b.Patient.LastName,
                        BookingDescription = b.Description,
                        BookingStatus = b.Status.ToString()
                    })
                    .ToListAsync();

                if (result.Count == 0)
                {
                    return NotFound($"No bookings found for doctor with ID {doctorId}");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching booking details.");
                return StatusCode(500, "Internal server error.");
            }
        }


        [Authorize]
        [HttpGet]
        [Route("api/[controller]/getBookingsWithReviews")]
        public async Task<IActionResult> GetBookingsWithReviews()
        {
            try
            {
                var DoctorEmail = HttpContext.User.FindFirst(ClaimTypes.Email);

                if (DoctorEmail == null)
                {
                    return NotFound("Doctor's email does not exist");
                }
                string email = DoctorEmail.Value;

                var doctor = await _context.Doctors
              .FirstOrDefaultAsync(p => p.Email == email);

                if (doctor == null)
                {
                    return NotFound("No doctor record found for the given email.");
                }

                var doctorId = doctor.Id;

                var result = await _context.Bookings
                    .Where(b => b.DoctorId == doctorId && b.Reviews.Any()) // Filter for bookings that have reviews
                    .Select(b => new
                    {
                        BookingId = b.BookingId,
                        DoctorName = b.Doctor.FirstName + " " + b.Doctor.LastName,
                        DoctorPrice = b.Doctor.Price,
                        PatientName = b.Patient.FirstName + " " + b.Patient.LastName,
                        BookingDescription = b.Description,
                        BookingStatus = b.Status.ToString(),
                        // Include review details directly in the booking object
                        ReviewDescription = b.Reviews.Select(r => r.Description).FirstOrDefault(),
                        ReviewStars = b.Reviews.Select(r => r.Stars).FirstOrDefault()
                    })
                    .ToListAsync();

                if (result.Count == 0)
                {
                    return NotFound($"No bookings found with reviews for doctor with ID {doctorId}");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching booking details.");
                return StatusCode(500, "Internal server error.");
            }
        }
        [Authorize]
        [HttpPut]
        [Route("api/[controller]/updateBookingStatus")]
        public async Task<IActionResult> UpdateBookingStatus(int bookingId, Status newStatus)
        {
            try
            {
                // Fetch the booking from the database using the bookingId
                var booking = await _context.Bookings
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                if (booking == null)
                {
                    return NotFound($"Booking with ID {bookingId} not found.");
                }

                // Update the status of the booking
                booking.Status = newStatus;

                // Save the changes in the database
                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                return Ok($"Booking status updated to {newStatus}.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the booking status.");
                return StatusCode(500, "Internal server error.");
            }
        }
        [Authorize]
        [HttpGet]
        [Route("api/[controller]/getDoctorInfo")]
        public async Task<IActionResult> GetDoctorInfo()
        {
            try
            {

                var DoctorEmail = HttpContext.User.FindFirst(ClaimTypes.Email);

                if (DoctorEmail == null)
                {
                    return NotFound("Doctor's email does not exist");
                }
                string email = DoctorEmail.Value;

                var Doctor = await _context.Doctors
              .FirstOrDefaultAsync(p => p.Email == email);

                if (Doctor == null)
                {
                    return NotFound("No doctor record found for the given email.");
                }

                var doctorId = Doctor.Id;
                // Fetch the doctor details from the database using the doctorId
                var doctor = await _context.Doctors
                    .Where(d => d.Id == doctorId)
                    .FirstOrDefaultAsync();

                // Check if doctor exists
                if (doctor == null)
                {
                    return NotFound($"Doctor with ID {doctorId} not found.");
                }

                // Return the doctor information, converting the Category enum to a string
                var doctorInfo = new
                {
                    doctor.FirstName,
                    doctor.LastName,
                    doctor.Email,
                    Category = doctor.Category.ToString(), // Convert enum to string
                    doctor.Price
                };

                return Ok(doctorInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching doctor information.");
                return StatusCode(500, "Internal server error.");
            }

         

        }

        [Authorize]
        [HttpGet]
        [Route("/api/[controller]/getReviews")]
        public async Task<IActionResult> GetReviews()
        {
            try
            {
                var DoctorEmail = HttpContext.User.FindFirst(ClaimTypes.Email);

                if (DoctorEmail == null)
                {
                    return NotFound("Doctor's email does not exist");
                }
                string email = DoctorEmail.Value;

                var doctor = await _context.Doctors
              .FirstOrDefaultAsync(p => p.Email == email);

                if (doctor == null)
                {
                    return NotFound("No doctor record found for the given email.");
                }

                var reviews = await _context.Reviews
                    .Include(r => r.Booking)
                    .Where(r => r.Booking.DoctorId == doctor.Id)
                    .Select(r => new
                    {
                        r.Id,
                        r.Description,
                        r.Stars,
                        BookingId = r.BookingId
                    }).ToListAsync();

                if (reviews.Count() == 0)
                {
                    return NotFound("No reviews found for the given doctor.");
                }

                return Ok(reviews);

            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching Reviews.");
                return StatusCode(500, "Internal server error.");
            }
        }







    }
}
