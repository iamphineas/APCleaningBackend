namespace APCleaningBackend.ViewModel
{
    public class UpdateUserModel
    {
        public string UserId { get; set; }
        public string FullName { get; set; }  // Full name of the user
        public string Email { get; set; }  // User's email
        public string PhoneNumber { get; set; }  // Optional phone number
    }
}
