﻿using Microsoft.VisualBasic;
using PurchasingSystemApps.Areas.MasterData.Models;
using PurchasingSystemApps.Models;
using PurchasingSystemApps.Repositories;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DueDate = PurchasingSystemApps.Areas.MasterData.Models.DueDate;

namespace PurchasingSystemApps.Areas.Order.Models
{
    [Table("OrdPurchaseRequest", Schema = "dbo")]
    public class PurchaseRequest : UserActivity
    {
        [Key]
        public Guid PurchaseRequestId { get; set; }
        public string PurchaseRequestNumber { get; set; }
        public string UserAccessId { get; set; }
        public Guid? UserApprove1Id { get; set; }
        public Guid? UserApprove2Id { get; set; }
        public Guid? UserApprove3Id { get; set; }
        public Guid? TermOfPaymentId { get; set; }
        public Guid? DueDateId { get; set; }
        public string Status { get; set; }
        public int QtyTotal { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Note { get; set; }
        public List<PurchaseRequestDetail> PurchaseRequestDetails { get; set; } = new List<PurchaseRequestDetail>();

        //Relationship
        [ForeignKey("UserAccessId")]
        public ApplicationUser? ApplicationUser { get; set; }     
        [ForeignKey("TermOfPaymentId")]
        public TermOfPayment? TermOfPayment { get; set; }
        [ForeignKey("UserApprove1Id")]
        public UserActive? UserApprove1 { get; set; }
        [ForeignKey("UserApprove2Id")]
        public UserActive? UserApprove2 { get; set; }
        [ForeignKey("UserApprove3Id")]
        public UserActive? UserApprove3 { get; set; }
        [ForeignKey("DueDateId")]
        public DueDate? DueDate { get; set; }
    }

    [Table("OrdPurchaseRequestDetail", Schema = "dbo")]
    public class PurchaseRequestDetail : UserActivity
    {
        [Key]
        public Guid PurchaseRequestDetailId { get; set; }
        public Guid? PurchaseRequestId { get; set; }
        public string ProductNumber { get; set; }
        public string ProductName { get; set; }        
        public string Measurement { get; set; }
        public string Principal { get; set; }
        public int Qty { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public decimal SubTotal { get; set; }

        //Relationship
        [ForeignKey("PurchaseRequestId")]
        public PurchaseRequest? PurchaseRequest { get; set; }
    }
}
