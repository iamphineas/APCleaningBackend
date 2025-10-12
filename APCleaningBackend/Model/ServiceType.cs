using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace APCleaningBackend.Model
{
    public class ServiceType
    {
        [BindNever]
        public int ServiceTypeID { get; set; }  // Primary Key
        public string Name { get; set; }  // E.g., "Home Cleaning", "Office Cleaning"
        public string Description { get; set; }  // Optional: Description of the service type
        public decimal Price { get; set; }  // Price of the service
        public string ImageURL { get; set; }  // Image url of service

    }
}
