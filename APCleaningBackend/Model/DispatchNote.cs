namespace APCleaningBackend.Model
{
    public class DispatchNote
    {
        public int Id { get; set; }
        public int BookingID { get; set; }
        public int DriverID { get; set; }
        public string Note { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    }
}
