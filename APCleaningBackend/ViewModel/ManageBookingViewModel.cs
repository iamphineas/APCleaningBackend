namespace APCleaningBackend.ViewModel
{
    public class ManageBookingViewModel
    {
        public int BookingID { get; set; }
        public string BookingStatus { get; set; }
        public bool CleanerCompleted { get; set; }
        public bool DriverCompleted { get; set; }
        public string AssignedCleanerName { get; set; }
        public string AssignedDriverName { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceName { get; set; }
    }
}
