namespace APCleaningBackend.ViewModel
{
    public class CleanerUpdateViewModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int ServiceTypeID { get; set; }
        public string AvailabilityStatus { get; set; } = "Available";
        public string CleanerImageUrl { get; set; }
    }
}
