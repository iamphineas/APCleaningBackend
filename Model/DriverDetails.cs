namespace APCleaningBackend.Model
{
    public class DriverDetails
    {
        public int DriverID { get; set; }  // Foreign Key to ApplicationUser (Driver)
        public string LicenseNumber { get; set; }  // License number
        public string VehicleType { get; set; }  // E.g., "Van", "SUV"
        public string AvailabilityStatus { get; set; }  // E.g., "Available", "Unavailable"

        // Navigation property
        public virtual ApplicationUser Driver { get; set; }
    }

}
