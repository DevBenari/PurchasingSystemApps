using Microsoft.EntityFrameworkCore;
using PurchasingSystemApps.Areas.MasterData.Models;
using PurchasingSystemApps.Data;

namespace PurchasingSystemApps.Areas.MasterData.Repositories
{
    public class IPositionRepository
    {
        private readonly ApplicationDbContext _context;

        public IPositionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Position Add(Position Position)
        {
            _context.Positions.Add(Position);
            _context.SaveChanges();
            return Position;
        }

        public Position Update(Position PositionChanges)
        {
            var Position = _context.Positions.Attach(PositionChanges);
            Position.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return PositionChanges;
        }

        public Position Delete(Guid Id)
        {
            var Position = _context.Positions.Find(Id);
            if (Position != null)
            {
                _context.Positions.Remove(Position);
                _context.SaveChanges();
            }
            return Position;
        }

        public IEnumerable<Position> GetAllPosition()
        {
            return _context.Positions
                .AsNoTracking();
        }

        public async Task<List<Position>> GetPositions()
        {
            return await _context.Positions.OrderBy(p => p.PositionName).Select(x => new Position()
            {
                PositionId = x.PositionId,
                PositionCode = x.PositionCode,
                PositionName = x.PositionName
            }).ToListAsync();
        }
    }
}
