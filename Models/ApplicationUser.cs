using Microsoft.AspNetCore.Identity;

namespace APCleaningBackend.Models
{
    public class ApplicationUser : IdentityUser
    {
      
        public string FullName { get; set; }
    }
}
