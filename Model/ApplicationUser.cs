using Microsoft.AspNetCore.Identity;

namespace APCleaningBackend.Model
{

    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }  // Full name of the user (Customer, Driver, Cleaner, etc.)
        public bool IsActive { get; set; } = true;  // Account active status
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;  // Account creation date
        public DateTime? LastLogin { get; set; }  // Timestamp for the last login

        // Navigation property (optional for querying)
        public virtual ICollection<Booking> Bookings { get; set; }  // List of bookings for customers
    }
}
