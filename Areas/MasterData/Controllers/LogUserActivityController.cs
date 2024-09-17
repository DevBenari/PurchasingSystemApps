using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PurchasingSystemApps.Areas.MasterData.Models;
using PurchasingSystemApps.Areas.MasterData.Repositories;
using PurchasingSystemApps.Areas.MasterData.ViewModels;
using PurchasingSystemApps.Data;
using PurchasingSystemApps.Models;
using System.Data.SqlClient;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace PurchasingSystemApps.Areas.MasterData.Controllers
{

    [Area("MasterData")]
    [Route("MasterData/[Controller]/[Action]")]
    public class LogUserActivityController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogUserActivityRepository _LogUserActivityRepository;
        private readonly IWarehouseLocationRepository _warehouseLocationRepository;
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IPositionRepository _positionRepository;


        private readonly IHostingEnvironment _hostingEnvironment;

        public LogUserActivityController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext applicationDbContext,
            ILogUserActivityRepository LogUserActivityRepository,
            IWarehouseLocationRepository warehouseLocationRepository,
            IDepartmentRepository departmentRepository,
            IPositionRepository positionRepository,

            IHostingEnvironment hostingEnvironment
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _applicationDbContext = applicationDbContext;
            _LogUserActivityRepository = LogUserActivityRepository;
            _warehouseLocationRepository = warehouseLocationRepository;
            _departmentRepository = departmentRepository;
            _positionRepository = positionRepository;

            _hostingEnvironment = hostingEnvironment;
        }

        public JsonResult LoadPosition(Guid Id)
        {
            var position = _applicationDbContext.Positions.Where(p => p.DepartmentId == Id).ToList();
            return Json(new SelectList(position, "PositionId", "PositionName"));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewBag.Active = "MasterData";
            var data = _LogUserActivityRepository.GetAllLogUserActivity();
            return View(data);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(DateTime tglAwalPencarian, DateTime tglAkhirPencarian)
        {
            ViewBag.Active = "MasterData";
            ViewBag.tglAwalPencarian = tglAwalPencarian.ToString("dd MMMM yyyy");
            ViewBag.tglAkhirPencarian = tglAkhirPencarian.ToString("dd MMMM yyyy");

            var data = _LogUserActivityRepository.GetAllLogUserActivity().Where(r => r.CreateDateTime.Date >= tglAwalPencarian && r.CreateDateTime.Date <= tglAkhirPencarian).ToList();
            return View(data);
        }

        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<ViewResult> CreateLogUserActivity()
        //{
        //    ViewBag.Active = "MasterData";

        //    ViewBag.Department = new SelectList(await _departmentRepository.GetDepartments(), "DepartmentId", "DepartmentName", SortOrder.Ascending);
        //    ViewBag.Position = new SelectList(await _positionRepository.GetPositions(), "PositionId", "PositionName", SortOrder.Ascending);

        //    var user = new LogUserActivityViewModel();
            

        //    return View(user);
        //}

        //[HttpPost]
        //[AllowAnonymous]
        //public async Task<IActionResult> CreateLogUserActivity(LogUserActivityViewModel vm)
        //{
        //    ViewBag.Department = new SelectList(await _departmentRepository.GetDepartments(), "DepartmentId", "DepartmentName", SortOrder.Ascending);
        //    ViewBag.Position = new SelectList(await _positionRepository.GetPositions(), "PositionId", "PositionName", SortOrder.Ascending);

        //    var getUser = _LogUserActivityRepository.GetAllUserLogin().Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

        //    if (ModelState.IsValid)
        //    {
        //        string uniqueFileName = ProcessUploadFile(vm);

        //        var user = new LogUserActivity
        //        {
        //            CreateDateTime = DateTime.Now,
        //            CreateBy = new Guid(getUser.Id),
        //            LogActiveId = vm.LogActiveId,
        //            LogLevel = vm.LogLevel,
        //            FullName = vm.FullName,
        //            Message = vm.Message,
        //            Timestamp = vm.Timestamp,
        //            IpAddres = vm.IpAddres,
        //            Action = vm.Action
        //        };

        //        var result = _LogUserActivityRepository.GetAllLogUserActivity().Where(c => c.LogActiveId == vm.LogActiveId).FirstOrDefault();
        //        if (result == null)
        //        {
        //            _LogUserActivityRepository.Tambah(user);
        //            TempData["SuccessMessage"] = "Name " + vm.FullName + " Saved";
        //            return RedirectToAction("Index", "Measurement");
        //        }
        //        else
        //        {
        //            TempData["WarningMessage"] = "Name " + vm.FullName + " Already Exist !!!";
        //            return View(vm);
        //        }


        //    }

        //    ViewBag.Department = new SelectList(await _departmentRepository.GetDepartments(), "DepartmentId", "DepartmentName", SortOrder.Ascending);
        //    ViewBag.Position = new SelectList(await _positionRepository.GetPositions(), "PositionId", "PositionName", SortOrder.Ascending);
        //    return View();
        //}

        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> DetailLogUserActivity(Guid Id)
        //{
        //    ViewBag.Department = new SelectList(await _departmentRepository.GetDepartments(), "DepartmentId", "DepartmentName", SortOrder.Ascending);
        //    ViewBag.Position = new SelectList(await _positionRepository.GetPositions(), "PositionId", "PositionName", SortOrder.Ascending);

        //    ViewBag.Active = "MasterData";
        //    var user = await _LogUserActivityRepository.GetUserById(Id);

        //    if (user == null)
        //    {
        //        Response.StatusCode = 404;
        //        return View("UserNotFound", Id);
        //    }

        //    LogUserActivityViewModel viewModel = new LogUserActivityViewModel
        //    {
        //        LogActiveId = user.LogActiveId,
        //        LogLevel = user.LogLevel,
        //        FullName = user.FullName,
        //        Message = user.Message,
        //        Timestamp = user.Timestamp,
        //        IpAddres = user.IpAddres,
        //        Action = user.Action
        //    };
        //    return View(viewModel);
        //}
    }
}
