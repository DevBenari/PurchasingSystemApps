using Microsoft.EntityFrameworkCore;
using PurchasingSystemApps.Areas.MasterData.Models;
using PurchasingSystemApps.Data;

namespace PurchasingSystemApps.Areas.MasterData.Repositories
{
    public class IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;

        public IDepartmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Department Add(Department Department)
        {
            _context.Departments.Add(Department);
            _context.SaveChanges();
            return Department;
        }

        public Department Update(Department DepartmentChanges)
        {
            var Department = _context.Departments.Attach(DepartmentChanges);
            Department.State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.SaveChanges();
            return DepartmentChanges;
        }

        public Department Delete(Guid Id)
        {
            var Department = _context.Departments.Find(Id);
            if (Department != null)
            {
                _context.Departments.Remove(Department);
                _context.SaveChanges();
            }
            return Department;
        }

        public IEnumerable<Department> GetAllDepartment()
        {
            return _context.Departments
                .Include(p => p.Positions)
                .AsNoTracking();
        }

        public async Task<List<Department>> GetDepartments()
        {
            return await _context.Departments.OrderBy(p => p.DepartmentName).Select(x => new Department()
            {
                DepartmentId = x.DepartmentId,
                DepartmentCode = x.DepartmentCode,
                DepartmentName = x.DepartmentName
            }).ToListAsync();
        }
    }
}
