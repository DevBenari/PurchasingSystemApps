using Microsoft.EntityFrameworkCore;
using PurchasingSystemApps.Areas.MasterData.Models;
using PurchasingSystemApps.Data;
using PurchasingSystemApps.Models;

namespace PurchasingSystemApps.Areas.MasterData.Repositories
{
    public class ILogUserActivityRepository
    {
        private readonly ApplicationDbContext _context;

        public ILogUserActivityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public LogUserActivity Tambah(LogUserActivity user)
        {
            _context.LogUserActivitys.Add(user);
            _context.SaveChanges();
            return user;
        }

        public async Task<LogUserActivity> GetUserById(Guid Id)
        {
            var user = await _context.LogUserActivitys                
                .SingleOrDefaultAsync(i => i.LogActiveId == Id);

            if (user != null)
            {
                var userDetail = new LogUserActivity()
                {
                    LogActiveId = user.LogActiveId,
                    FullName = user.FullName,
                    LogLevel = user.LogLevel,
                    Message = user.Message,
                    Timestamp = user.Timestamp,
                    IpAddres = user.IpAddres,
                    Action = user.Action
                };
                return userDetail;
            }
            return null;
        }

        public async Task<LogUserActivity> GetUserByIdNoTracking(Guid Id)
        {
            return await _context.LogUserActivitys.AsNoTracking().FirstOrDefaultAsync(a => a.LogActiveId == Id);
        }

        public async Task<List<LogUserActivity>> GetLogUserActivitys()
        {
            return await _context.LogUserActivitys.OrderBy(p => p.CreateDateTime).Where(u => u.FullName != "Administrator").Select(user => new LogUserActivity()
            {
                LogActiveId = user.LogActiveId,
                FullName = user.FullName,
                LogLevel = user.LogLevel,
                Message = user.Message,
                Timestamp = user.Timestamp,
                IpAddres = user.IpAddres,
                Action = user.Action
            }).ToListAsync();
        }

        public IEnumerable<LogUserActivity> GetAllLogUserActivity()
        {
            return _context.LogUserActivitys.OrderByDescending(d => d.CreateDateTime)
                .AsNoTracking();
        }

        public IEnumerable<ApplicationUser> GetAllUserLogin()
        {
            return _context.Users
                .AsNoTracking();
        }

        public LogUserActivity Update(LogUserActivity update)
        {
            var user = _context.LogUserActivitys.Attach(update);
            user.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return update;
        }

        public LogUserActivity Delete(Guid Id)
        {
            var user = _context.LogUserActivitys.Find(Id);
            if (user != null)
            {
                _context.LogUserActivitys.Remove(user);
                _context.SaveChanges();
            }
            return user;
        }
    }
}
