﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchasingSystemApps.Areas.MasterData.Models;
using PurchasingSystemApps.Areas.MasterData.Repositories;
using PurchasingSystemApps.Areas.MasterData.ViewModels;
using PurchasingSystemApps.Data;
using PurchasingSystemApps.Models;
using PurchasingSystemApps.Repositories;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace PurchasingSystemApps.Areas.MasterData.Controllers
{
    [Area("MasterData")]
    [Route("MasterData/[Controller]/[Action]")]
    public class UnitLocationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IUserActiveRepository _userActiveRepository;
        private readonly IUnitLocationRepository _UnitLocationRepository;
        private readonly IProductRepository _productRepository;

        private readonly IHostingEnvironment _hostingEnvironment;

        public UnitLocationController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext applicationDbContext,
            IUnitLocationRepository UnitLocationRepository,
            IUserActiveRepository userActiveRepository,
            IProductRepository productRepository,

            IHostingEnvironment hostingEnvironment
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _applicationDbContext = applicationDbContext;
            _UnitLocationRepository = UnitLocationRepository;
            _userActiveRepository = userActiveRepository;
            _productRepository = productRepository;

            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            ViewBag.Active = "MasterData";
            var data = _UnitLocationRepository.GetAllUnitLocation();
            return View(data);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(DateTime tglAwalPencarian, DateTime tglAkhirPencarian)
        {
            ViewBag.Active = "MasterData";
            ViewBag.tglAwalPencarian = tglAwalPencarian.ToString("dd MMMM yyyy");
            ViewBag.tglAkhirPencarian = tglAkhirPencarian.ToString("dd MMMM yyyy");

            var data = _UnitLocationRepository.GetAllUnitLocation().Where(r => r.CreateDateTime.Date >= tglAwalPencarian && r.CreateDateTime.Date <= tglAkhirPencarian).ToList();
            return View(data);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ViewResult> CreateUnitLocation()
        {
            ViewBag.Active = "MasterData";

            ViewBag.UnitManager = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);

            var user = new UnitLocationViewModel();
            var dateNow = DateTimeOffset.Now;
            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            var lastCode = _UnitLocationRepository.GetAllUnitLocation().Where(d => d.CreateDateTime.ToString("yyMMdd") == dateNow.ToString("yyMMdd")).OrderByDescending(k => k.UnitLocationCode).FirstOrDefault();
            if (lastCode == null)
            {
                user.UnitLocationCode = "UNT" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.UnitLocationCode.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    user.UnitLocationCode = "UNT" + setDateNow + "0001";
                }
                else
                {
                    user.UnitLocationCode = "UNT" + setDateNow + (Convert.ToInt32(lastCode.UnitLocationCode.Substring(9, lastCode.UnitLocationCode.Length - 9)) + 1).ToString("D4");
                }
            }

            return View(user);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateUnitLocation(UnitLocationViewModel vm)
        {
            var dateNow = DateTimeOffset.Now;
            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            var lastCode = _UnitLocationRepository.GetAllUnitLocation().Where(d => d.CreateDateTime.ToString("yyMMdd") == dateNow.ToString("yyMMdd")).OrderByDescending(k => k.UnitLocationCode).FirstOrDefault();
            if (lastCode == null)
            {
                vm.UnitLocationCode = "UNT" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.UnitLocationCode.Substring(3, 6);

                if (lastCodeTrim != setDateNow)
                {
                    vm.UnitLocationCode = "UNT" + setDateNow + "0001";
                }
                else
                {
                    vm.UnitLocationCode = "UNT" + setDateNow + (Convert.ToInt32(lastCode.UnitLocationCode.Substring(9, lastCode.UnitLocationCode.Length - 9)) + 1).ToString("D4");
                }
            }

            var getUser = _userActiveRepository.GetAllUserLogin().Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            if (ModelState.IsValid)
            {
                var UnitLocation = new UnitLocation
                {
                    CreateDateTime = DateTime.Now,
                    CreateBy = new Guid(getUser.Id),
                    UnitLocationId = vm.UnitLocationId,
                    UnitLocationCode = vm.UnitLocationCode,
                    UnitLocationName = vm.UnitLocationName,
                    UnitManagerId = vm.UnitManagerId,
                    Address = vm.Address
                };

                var result = _UnitLocationRepository.GetAllUnitLocation().Where(c => c.UnitLocationName == vm.UnitLocationName).FirstOrDefault();
                if (result == null)
                {
                    _UnitLocationRepository.Tambah(UnitLocation);
                    TempData["SuccessMessage"] = "Name " + vm.UnitLocationName + " Saved";
                    return RedirectToAction("Index", "UnitLocation");
                }
                else
                {
                    ViewBag.UnitManager = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);
                    TempData["WarningMessage"] = "Name " + vm.UnitLocationName + " Already Exist !!!";
                    return View(vm);
                }

            }
            ViewBag.UnitManager = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DetailUnitLocation(Guid Id)
        {
            ViewBag.Active = "MasterData";

            ViewBag.UnitManager = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);

            var UnitLocation = await _UnitLocationRepository.GetUnitLocationById(Id);

            if (UnitLocation == null)
            {
                Response.StatusCode = 404;
                return View("UserNotFound", Id);
            }

            UnitLocationViewModel viewModel = new UnitLocationViewModel
            {
                UnitLocationId = UnitLocation.UnitLocationId,
                UnitLocationCode = UnitLocation.UnitLocationCode,
                UnitLocationName = UnitLocation.UnitLocationName,
                UnitManagerId = UnitLocation.UnitManagerId,
                Address = UnitLocation.Address
            };
            return View(viewModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> DetailUnitLocation(UnitLocationViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var UnitLocation = await _UnitLocationRepository.GetUnitLocationByIdNoTracking(viewModel.UnitLocationId);
                var getUser = _userActiveRepository.GetAllUserLogin().Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
                var check = _UnitLocationRepository.GetAllUnitLocation().Where(d => d.UnitLocationCode == viewModel.UnitLocationCode).FirstOrDefault();

                if (check != null)
                {
                    UnitLocation.UpdateDateTime = DateTime.Now;
                    UnitLocation.UpdateBy = new Guid(getUser.Id);
                    UnitLocation.UnitLocationCode = viewModel.UnitLocationCode;
                    UnitLocation.UnitLocationName = viewModel.UnitLocationName;
                    UnitLocation.UnitManagerId = viewModel.UnitManagerId;
                    UnitLocation.Address = viewModel.Address;

                    _UnitLocationRepository.Update(UnitLocation);
                    _applicationDbContext.SaveChanges();

                    TempData["SuccessMessage"] = "Name " + viewModel.UnitLocationName + " Success Changes";
                    return RedirectToAction("Index", "UnitLocation");
                }
                else
                {
                    ViewBag.UnitManager = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);

                    TempData["WarningMessage"] = "Name " + viewModel.UnitLocationName + " Already Exist !!!";
                    return View(viewModel);
                }
            }
            ViewBag.UnitManager = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        //[Authorize]
        public async Task<IActionResult> DeleteUnitLocation(Guid Id)
        {
            ViewBag.Active = "MasterData";
            var UnitLocation = await _UnitLocationRepository.GetUnitLocationById(Id);
            if (UnitLocation == null)
            {
                Response.StatusCode = 404;
                return View("UnitLocationNotFound", Id);
            }

            UnitLocationViewModel vm = new UnitLocationViewModel
            {
                UnitLocationId = UnitLocation.UnitLocationId,
                UnitLocationCode = UnitLocation.UnitLocationCode,
                UnitLocationName = UnitLocation.UnitLocationName,
            };
            return View(vm);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> DeleteUnitLocation(UnitLocationViewModel vm)
        {
            //Hapus Data
            var UnitLocation = _applicationDbContext.UnitLocations.FirstOrDefault(x => x.UnitLocationId == vm.UnitLocationId);
            _applicationDbContext.Attach(UnitLocation);
            _applicationDbContext.Entry(UnitLocation).State = EntityState.Deleted;
            _applicationDbContext.SaveChanges();

            TempData["SuccessMessage"] = "Name " + vm.UnitLocationName + " Success Deleted";
            return RedirectToAction("Index", "UnitLocation");
        }
    }
}