using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json.Serialization;

namespace APCleaningBackend.Model
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        // --- Customer ---
        [Required]
        public string CustomerID { get; set; }

        [ForeignKey(nameof(CustomerID))]
        [JsonIgnore]
        [BindNever] // ✅ Prevent model binding from validating this
        public virtual ApplicationUser Customer { get; set; }

        // --- Driver ---
        public int? AssignedDriverID { get; set; }

        [ForeignKey(nameof(AssignedDriverID))]
        [JsonIgnore]
        [BindNever]
        public virtual DriverDetails AssignedDriver { get; set; }

        // --- Cleaner ---
        public int? AssignedCleanerID { get; set; }

        [ForeignKey(nameof(AssignedCleanerID))]
        [JsonIgnore]
        [BindNever]
        public virtual CleanerDetails AssignedCleaner { get; set; }

        // --- Service Type ---
        [Required]
        public int ServiceTypeID { get; set; }

        [ForeignKey(nameof(ServiceTypeID))]
        [JsonIgnore]
        [BindNever]
        public virtual ServiceType ServiceType { get; set; }

        // --- Booking Info ---
        [Required]
        public DateTime ServiceDate { get; set; }

        [Required]
        public DateTime ServiceStartTime { get; set; }

        [Required]
        public DateTime ServiceEndTime { get; set; }

        [Required]
        public decimal BookingAmount { get; set; }

        public string BookingStatus { get; set; } = "Pending";
        public string PaymentStatus { get; set; } = "Pending";
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string Address { get; set; }

        [Required]
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string Province { get; set; } = "Durban";
        public string FullName { get; set; }
        public string Email { get; set; }

    }
}