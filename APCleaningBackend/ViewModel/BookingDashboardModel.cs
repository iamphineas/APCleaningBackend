namespace APCleaningBackend.ViewModel
{
    public class BookingDashboardModel
    {
        public int BookingID { get; set; }
        public DateTime ServiceDate { get; set; }
        public DateTime ServiceStartTime { get; set; }
        public DateTime ServiceEndTime { get; set; }
        public string BookingStatus { get; set; }
        public string ServiceName { get; set; }

        // Dashboard-specific flags
        public bool CleanerCompleted { get; set; }
        public bool DriverCompleted { get; set; }

    }
}
