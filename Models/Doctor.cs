namespace SmartAppointmentSystem.Models
{
    public enum Category
    {
        General, Dentist, Dermatologist, Neurologist
    }
    public class Doctor
    {   
        public int Id { get; set; }
        public string Email { get; set; }   
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Price { get; set; }
        public Category Category { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
}
