namespace APCleaningBackend.Model
{
    public class CleanerDetails
    {
        public int CleanerID { get; set; }  // Foreign Key to ApplicationUser (Cleaner)
        public string Specialty { get; set; }  // E.g., "Deep Clean", "Carpet Cleaning"

        // Navigation property
        public virtual ApplicationUser Cleaner { get; set; }

        // You can store the time slots directly here or dynamically check bookings
        public virtual ICollection<Booking> Bookings { get; set; }  // All bookings for this cleaner
    }
}
