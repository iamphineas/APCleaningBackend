namespace APCleaningBackend.ViewModel
{
    public class ResetPasswordModel
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? CurrentPassword { get; set; }
        public string NewPassword { get; set; }

    }
}
