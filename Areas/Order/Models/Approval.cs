using PurchasingSystemApps.Areas.MasterData.Models;
using PurchasingSystemApps.Models;
using PurchasingSystemApps.Repositories;
using System.ComponentModel.DataAnnotations.Schema;

namespace PurchasingSystemApps.Areas.Order.Models
{
    [Table("OrdApproval", Schema = "dbo")]
    public class Approval : UserActivity
    {
        public Guid ApprovalId { get; set; }
        public Guid? PurchaseRequestId { get; set; }
        public string PurchaseRequestNumber { get; set; }
        public string UserAccessId { get; set; } //Dibuat Oleh
        public Guid? DueDateId { get; set; }
        //User 1
        public Guid? UserApprove1Id { get; set; }
        public string ApproveByUser1 { get; set; }
        public string User1ApproveTime { get; set; }        
        public DateTime User1ApproveDate { get; set; }
        //User 2
        public Guid? UserApprove2Id { get; set; }
        public string ApproveByUser2 { get; set; }
        public string User2ApproveTime { get; set; }
        public DateTime User2ApproveDate { get; set; }
        //User 3
        public Guid? UserApprove3Id { get; set; }
        public string ApproveByUser3 { get; set; }
        public string User3ApproveTime { get; set; }
        public DateTime User3ApproveDate { get; set; }        
        public string Status { get; set; }
        public string? Note { get; set; }

        //Relationship
        [ForeignKey("PurchaseRequestId")]
        public PurchaseRequest? PurchaseRequest { get; set; }        
        [ForeignKey("UserAccessId")]
        public ApplicationUser? ApplicationUser { get; set; }
        [ForeignKey("UserApprove1Id")]
        public UserActive? UserApprove1 { get; set; }
        [ForeignKey("UserApprove2Id")]
        public UserActive? UserApprove2 { get; set; }
        [ForeignKey("UserApprove3Id")]
        public UserActive? UserApprove3 { get; set; }
        [ForeignKey("DueDateId")]
        public DueDate? DueDate { get; set; }
    }
}
