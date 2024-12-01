namespace SmartAppointmentSystem.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public int Stars { get; set; }
        public int BookingId { get; set; }
        public Booking Booking { get; set; }
    }
}
