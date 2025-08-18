namespace APCleaningBackend.Model
{
    public class ServiceType
    {
        public int ServiceTypeID { get; set; }  // Primary Key
        public string Name { get; set; }  // E.g., "Home Cleaning", "Office Cleaning"
        public string Description { get; set; }  // Optional: Description of the service type

        // Navigation property (if needed)
        public virtual ICollection<Booking> Bookings { get; set; }  // Navigation to Bookings (One-to-Many relationship)
    }
}
