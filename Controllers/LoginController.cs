using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAppointmentSystem.Data;
using SmartAppointmentSystem.Authentication;
using SmartAppointmentSystem.DTOs;
using System.Security.Cryptography;
using System.Text;
using SmartAppointmentSystem.DTOs;
namespace SmartAppointmentSystem.Controllers
{
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly SmartAppointmentSystemContext _context;
        private readonly TokenService _tokenService;

        public LoginController(ILogger<LoginController> logger, SmartAppointmentSystemContext context, TokenService tokenService)
        {
            _logger = logger;
            _context = context;
            _tokenService = tokenService;
        }

        [HttpPost]
        [Route("api/[controller]/doctor")]
        public async Task<IActionResult> Login([FromBody] LoginDTO request)
        {
            try
            {
                // Hash the incoming password for comparison
                var hashedPassword = HashPassword(request.Password);

                // Query the Doctors table to find a match
                var doctor = await _context.Doctors
                    .FirstOrDefaultAsync(d => d.Email == request.Email && d.Password == hashedPassword);

                if (doctor == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                // Generate JWT token
                var token = _tokenService.GenerateToken(doctor.Id.ToString(), doctor.Email);

                // Return the token and doctor info
                return Ok(new
                {
                    Token = token,
                    DoctorId = doctor.Id,
                    FirstName = doctor.FirstName,
                    LastName= doctor.LastName,
                    Email = doctor.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        [Route("api/[controller]/patient")]
        public async Task<IActionResult> LoginPatient([FromBody] LoginDTO request)
        {
            try
            {
                // Hash the incoming password for comparison
                var hashedPassword = HashPassword(request.Password);

                // Query the Doctors table to find a match
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(d => d.Email == request.Email && d.Password == hashedPassword);

                if (patient == null)
                {
                    return Unauthorized("Invalid email or password.");
                }

                // Generate JWT token
                var token = _tokenService.GenerateToken(patient.Id.ToString(), patient.Email);

                // Return the token and doctor info
                return Ok(new
                {
                    Token = token,
                    DoctorId = patient.Id,
                    FirstName = patient.FirstName,
                    LastName = patient.LastName,
                    Email = patient.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                return StatusCode(500, "Internal server error.");
            }
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }

}
