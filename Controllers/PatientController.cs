using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAppointmentSystem.DTOs;
using SmartAppointmentSystem.Data;
using SmartAppointmentSystem.Models;
using System.Text;

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


    }
}
