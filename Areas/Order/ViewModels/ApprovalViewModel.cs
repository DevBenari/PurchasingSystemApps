using PurchasingSystemApps.Areas.Order.Models;

namespace PurchasingSystemApps.Areas.Order.ViewModels
{
    public class ApprovalViewModel
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
        public int QtyTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public List<PurchaseRequestDetail> PurchaseRequestDetails { get; set; }
    }
}
