using System.ComponentModel.DataAnnotations;

namespace APCleaningBackend.Model
{
    public class CleanerDetails
    {
        [Key]
        public int CleanerDetailsID { get; set; }  // Primary key for this table

        [Required]
        public string UserId { get; set; }  // FK to ApplicationUser.Id (string GUID)

        public int ServiceTypeID { get; set; }  // E.g., "Deep Clean", "Carpet Cleaning"
        public string AvailabilityStatus { get; set; } = "Available";  // E.g., "Available", "Unavailable"
        public string CleanerImageUrl { get; set; }
        public virtual ApplicationUser User { get; set; }

        // Cleaner-specific bookings
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }

}
