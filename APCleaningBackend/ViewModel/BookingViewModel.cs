namespace APCleaningBackend.ViewModel
{
    public class BookingViewModel
    {
        public string CustomerID { get; set; }
        public int? AssignedDriverID { get; set; }
        public int? AssignedCleanerID { get; set; }
        public int ServiceTypeID { get; set; }
        public DateTime ServiceDate { get; set; }
        public DateTime ServiceStartTime { get; set; }
        public DateTime ServiceEndTime { get; set; }
        public decimal BookingAmount { get; set; }
        public string BookingStatus { get; set; } = "Pending";
        public string PaymentStatus { get; set; } = "Pending";
        public DateTime CreatedDate { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Province { get; set; } = "Durban";

    }
}
