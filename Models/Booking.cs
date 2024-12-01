namespace SmartAppointmentSystem.Models
{   
    public enum Status
    {
        accepted, rejected, pending
    }
    public class Booking
    {
        public int BookingId { get; set; }
        public int DoctorId { get; set; }
        public string Description { get; set; }
        public Status Status { get; set; }
        public int PatientId { get; set; }
        public Patient Patient { get; set; }
        public Doctor Doctor { get; set; }
        public ICollection<Review> Reviews { get; set; }
    }
}
