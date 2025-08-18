namespace APCleaningBackend.Model
{
    public class Booking
    {
        public int BookingID { get; set; }
        public int CustomerID { get; set; }  // Foreign Key to ApplicationUser (Customer)
        public int? AssignedDriverID { get; set; }  // Foreign Key to ApplicationUser (Driver)
        public int? AssignedCleanerID { get; set; }  // Foreign Key to ApplicationUser (Cleaner)

        // Foreign Key to ServiceType
        public int ServiceTypeID { get; set; }  // Foreign Key to ServiceType
        public DateTime ServiceDate { get; set; }  // Date when service will happen
        public DateTime ServiceStartTime { get; set; } // Start time of the service
        public DateTime ServiceEndTime { get; set; }  // End time of the service
        public decimal BookingAmount { get; set; }  // Cost of the booking
        public string BookingStatus { get; set; }  // E.g., "Pending", "Assigned", "Completed"
        public string PaymentStatus { get; set; }  // E.g., "Paid", "Pending", "Failed"
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;  // Timestamp of booking creation

        // Navigation properties
        public virtual ApplicationUser Customer { get; set; }
        public virtual ApplicationUser AssignedDriver { get; set; }
        public virtual ApplicationUser AssignedCleaner { get; set; }

        // Navigation property to ServiceType
        public virtual ServiceType ServiceType { get; set; }  // Navigation to ServiceType
    }
}
