namespace APCleaningBackend.ViewModel
{
    public class CreateWorkerModel
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }  // "Driver" or "Cleaner"
        public string Password { get; set; }

        // Additional fields for Driver/Cleaner specific information
        public string LicenseNumber { get; set; }  // For drivers
        public string VehicleType { get; set; }  // For drivers
        public string Specialty { get; set; }  // For cleaners
    }

}
