namespace APCleaningBackend.Model
{
    public class Feedback
    {
        public int FeedbackID { get; set; }
        public int BookingID { get; set; }  // Foreign Key to Booking
        public int CustomerID { get; set; }  // Foreign Key to ApplicationUser (Customer)
        public int Rating { get; set; }  // Rating from 1 to 5
        public string Comments { get; set; }  // Optional feedback from the customer
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;  // Timestamp when feedback was provided

        // Navigation properties
        public virtual Booking Booking { get; set; }
        public virtual ApplicationUser Customer { get; set; }
    }
}
