namespace APCleaningBackend.Model
{
    public class Feedback
    {
        public int FeedbackID { get; set; }
        public string CustomerID { get; set; }  // Foreign Key to ApplicationUser (Customer)
        public int CleanerID { get; set; }
        public int Rating { get; set; }  // Rating from 1 to 5
        public string Comments { get; set; }  // Optional feedback from the customer
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;  // Timestamp when feedback was provided
    }
}
