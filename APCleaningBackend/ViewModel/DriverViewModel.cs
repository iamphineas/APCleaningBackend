namespace APCleaningBackend.ViewModel
{
    public class DriverViewModel
    {
        public int DriverDetailsID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public int ServiceTypeID { get; set; }
        public string LicenseNumber { get; set; }
        public string VehicleType { get; set; }
        public string AvailabilityStatus { get; set; } = "Available";
        public string DriverImageUrl { get; set; }

    }
}
