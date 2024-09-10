using Microsoft.EntityFrameworkCore;
using PurchasingSystemApps.Areas.Order.Models;
using PurchasingSystemApps.Data;

namespace PurchasingSystemApps.Areas.Order.Repositories
{
    public class IApprovalRepository
    {
        private string _errors = "";
        private readonly ApplicationDbContext _context;

        public IApprovalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public string GetErrors()
        {
            return _errors;
        }

        public Approval Tambah(Approval Approval)
        {
            _context.Approvals.Add(Approval);
            _context.SaveChanges();
            return Approval;
        }

        public async Task<Approval> GetApprovalById(Guid Id)
        {
            var Approval = _context.Approvals
                .Where(i => i.ApprovalId == Id)
                .Include(u => u.ApplicationUser)
                .Include(t => t.PurchaseRequest)
                .Include(a1 => a1.UserApprove1)
                .Include(a2 => a2.UserApprove2)
                .Include(a3 => a3.UserApprove3)
                .Include(e => e.DueDate)
                .FirstOrDefault(p => p.ApprovalId == Id);

            if (Approval != null)
            {
                var ApprovalDetail = new Approval()
                {
                    ApprovalId = Approval.ApprovalId,
                    PurchaseRequestId = Approval.PurchaseRequestId,
                    PurchaseRequestNumber = Approval.PurchaseRequestNumber,
                    UserAccessId = Approval.UserAccessId,
                    ApplicationUser = Approval.ApplicationUser,
                    //User 1
                    UserApprove1Id = Approval.UserApprove1Id,
                    ApproveByUser1 = Approval.ApproveByUser1,
                    User1ApproveTime = Approval.User1ApproveTime,
                    User1ApproveDate = Approval.User1ApproveDate,
                    //User 2
                    UserApprove2Id = Approval.UserApprove2Id,
                    ApproveByUser2 = Approval.ApproveByUser2,
                    User2ApproveTime = Approval.User2ApproveTime,
                    User2ApproveDate = Approval.User2ApproveDate,
                    //User 3
                    UserApprove3Id = Approval.UserApprove3Id,
                    ApproveByUser3 = Approval.ApproveByUser3,
                    User3ApproveTime = Approval.User3ApproveTime,
                    User3ApproveDate = Approval.User3ApproveDate,
                    
                    Status = Approval.Status,
                    Note = Approval.Note
                };
                return ApprovalDetail;
            }
            return Approval;
        }

        public async Task<Approval> GetApprovalByIdNoTracking(Guid Id)
        {
            return await _context.Approvals.AsNoTracking().Where(i => i.ApprovalId == Id).FirstOrDefaultAsync(a => a.ApprovalId == Id);
        }

        public async Task<List<Approval>> GetApprovals()
        {
            return await _context.Approvals./*OrderBy(p => p.CreateDateTime).*/Select(Approval => new Approval()
            {
                ApprovalId = Approval.ApprovalId,
                PurchaseRequestId = Approval.PurchaseRequestId,
                PurchaseRequestNumber = Approval.PurchaseRequestNumber,
                UserAccessId = Approval.UserAccessId,
                ApplicationUser = Approval.ApplicationUser,
                //User 1
                UserApprove1Id = Approval.UserApprove1Id,
                ApproveByUser1 = Approval.ApproveByUser1,
                User1ApproveTime = Approval.User1ApproveTime,
                User1ApproveDate = Approval.User1ApproveDate,
                //User 2
                UserApprove2Id = Approval.UserApprove2Id,
                ApproveByUser2 = Approval.ApproveByUser2,
                User2ApproveTime = Approval.User2ApproveTime,
                User2ApproveDate = Approval.User2ApproveDate,
                //User 3
                UserApprove3Id = Approval.UserApprove3Id,
                ApproveByUser3 = Approval.ApproveByUser3,
                User3ApproveTime = Approval.User3ApproveTime,
                User3ApproveDate = Approval.User3ApproveDate,

                Status = Approval.Status,
                Note = Approval.Note
            }).ToListAsync();
        }

        public IEnumerable<Approval> GetAllApproval()
        {
            return _context.Approvals.OrderByDescending(c => c.CreateDateTime)
                .Include(u => u.ApplicationUser)
                .Include(t => t.PurchaseRequest)
                .Include(a1 => a1.UserApprove1)
                .Include(a2 => a2.UserApprove2)
                .Include(a3 => a3.UserApprove3)
                .Include(e => e.DueDate)
                .ToList();
        }

        public async Task<Approval> Update(Approval update)
        {          
            var Approval = _context.Approvals.Attach(update);
            Approval.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return update;
        }

        public Approval Delete(Guid Id)
        {
            var Approval = _context.Approvals.Find(Id);
            if (Approval != null)
            {
                _context.Approvals.Remove(Approval);
                _context.SaveChanges();
            }
            return Approval;
        }
    }
}
