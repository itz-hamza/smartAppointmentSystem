using SmartAppointmentSystem.Models;
namespace SmartAppointmentSystem.Data
{
        public static class DbInitializer
        {
            public static void Initialize(SmartAppointmentSystemContext context)
            {
                // Check if there are any doctors already in the database
                if (context.Doctors.Any())
                {
                    return; // DB has been seeded
                }

                var doctors = new Doctor[]
                {
                new Doctor{Id=1, FirstName="Alice", LastName="Smith", Email="alice.smith@example.com", Password="password123", Price=100, Category=Category.General},
                new Doctor{Id=2, FirstName="Bob", LastName="Jones", Email="bob.jones@example.com", Password="password123", Price=150, Category=Category.Dentist}
                };

                context.Doctors.AddRange(doctors);
                context.SaveChanges();

                var patients = new Patient[]
                {
                new Patient{Id=1, FirstName="John", LastName="Doe", Email="john.doe@example.com", Password="password123"},
                new Patient{Id=2, FirstName="Jane", LastName="Doe", Email="jane.doe@example.com", Password="password123"}
                };

                context.Patients.AddRange(patients);
                context.SaveChanges();

                var bookings = new Booking[]
                {
                new Booking{BookingId=1, DoctorId=1, Description="General Checkup", Status=Status.pending, PatientId=1},
                new Booking{BookingId=2, DoctorId=2, Description="Dental Cleaning", Status=Status.accepted, PatientId=2}
                };

                context.Bookings.AddRange(bookings);
                context.SaveChanges();

                var reviews = new Review[]
                {
                new Review{Id=1, Description="Great service!", Stars=5, BookingId=1},
                new Review{Id=2, Description="Very professional!", Stars=4, BookingId=2}
                };

                context.Reviews.AddRange(reviews);
                context.SaveChanges();
            }
        }
    
}
