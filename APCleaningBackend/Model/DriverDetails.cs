using System.ComponentModel.DataAnnotations;

namespace APCleaningBackend.Model
{
    public class DriverDetails
    {
        [Key]
        public int DriverDetailsID { get; set; }  // Primary key for this table

        [Required]
        public string UserId { get; set; }  // FK to ApplicationUser.Id (string GUID)

        public string LicenseNumber { get; set; }  // License number
        public string VehicleType { get; set; }    // E.g., "Van", "SUV"
        public string AvailabilityStatus { get; set; } = "Available";  // E.g., "Available", "Unavailable"
    }


}
