namespace PurchasingSystemApps.Areas.MasterData.ViewModels
{
    public class LogUserActivityViewModel
    {
        public Guid LogActiveId { get; set; }
        public string FullName { get; set; }
        public string LogLevel { get; set; }
        public string? Message { get; set; }
        public string? Timestamp { get; set; }
        public string? IpAddres { get; set; }
        public string Action { get; set; }
        public string LogUserActivityCode { get; internal set; }
    }
}
