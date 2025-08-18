namespace APCleaningBackend.Model
{
    public class PaymentTransaction
    {
        public int PaymentTransactionID { get; set; }
        public int BookingID { get; set; }  // Foreign Key to Booking
        public decimal Amount { get; set; }  // Amount paid for the service
        public string PaymentMethod { get; set; }  // E.g., "PayFast", "PayPal", "Credit Card"
        public string PaymentStatus { get; set; }  // E.g., "Success", "Failed"
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;  // Timestamp when the payment was made

        // Navigation property
        public virtual Booking Booking { get; set; }
    }

}
