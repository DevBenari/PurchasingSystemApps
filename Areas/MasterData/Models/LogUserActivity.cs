using PurchasingSystemApps.Repositories;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PurchasingSystemApps.Areas.MasterData.Models
{
    [Table("LogUserActivity", Schema = "dbo")]
    public class LogUserActivity : UserActivity
    {       
            [Key]
            public Guid LogActiveId { get; set; }
            public string FullName { get; set; }
            public string LogLevel { get; set; }
            public string? Message { get; set; }
            public string? Timestamp { get; set; }
            public string? IpAddres { get; set; }
            public string Action { get; set; }
        
    }
}
