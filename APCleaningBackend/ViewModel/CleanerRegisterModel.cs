namespace APCleaningBackend.ViewModel
{
    public class CleanerRegisterModel
    {
        public int CleanerDetailsID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public int ServiceTypeID { get; set; }
        public string AvailabilityStatus { get; set; } = "Available";
    }
}
