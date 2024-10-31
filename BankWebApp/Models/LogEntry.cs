namespace BankWebApp.Models
{
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string AdminUsername { get; set; }
        public string Action { get; set; }
        public string Details { get; set; }
    }
}
