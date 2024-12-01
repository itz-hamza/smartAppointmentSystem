using Microsoft.EntityFrameworkCore;
using SmartAppointmentSystem.Models;

namespace SmartAppointmentSystem.Data
{
    public class SmartAppointmentSystemContext : DbContext
    {
        public SmartAppointmentSystemContext(DbContextOptions<SmartAppointmentSystemContext> options) : base(options)
        {

        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Review> Reviews { get; set; }

        // Map entities to database tables
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Doctor>().ToTable("Doctor");
            modelBuilder.Entity<Patient>().ToTable("Patient");
            modelBuilder.Entity<Booking>().ToTable("Booking");
            modelBuilder.Entity<Review>().ToTable("Review"); 
        }
    }
}
