namespace APCleaningBackend.Model
{
    public class WaitlistEntry
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    }
}
