using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PurchasingSystemApps.Areas.MasterData.Repositories;
using PurchasingSystemApps.Areas.MasterData.ViewModels;
using PurchasingSystemApps.Data;
using PurchasingSystemApps.Models;
using PurchasingSystemApps.Repositories;

namespace PurchasingSystemApps.Areas.MasterData.Controllers
{
    [Area("MasterData")]
    [Route("MasterData/[Controller]/[Action]")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IUserActiveRepository _userActiveRepository;
        private readonly IProductRepository _productRepository;

        public DashboardController(
            ApplicationDbContext applicationDbContext,
            IUserActiveRepository userActiveRepository,
            IProductRepository productRepository
        )
        {
            _applicationDbContext = applicationDbContext;
            _userActiveRepository = userActiveRepository;
            _productRepository    = productRepository;
        }
        public IActionResult Index()
        {
            ViewBag.Active = "MasterData";
            var countUser = _applicationDbContext.UserActives.GroupBy(u => u.UserActiveId).Select(y => new
            {
                UserActiveId = y.Key,
                CountOfUsers = y.Count()
            }).ToList();
            ViewBag.CountUser = countUser.Count;

            var countPrincipal = _applicationDbContext.Principals.GroupBy(u => u.PrincipalId).Select(y => new
            {
                PrincipalId = y.Key,
                CountOfPrincipals = y.Count()
            }).ToList();
            ViewBag.CountPrincipal = countPrincipal.Count;

            var countProduct = _applicationDbContext.Products.GroupBy(u => u.ProductId).Select(y => new
            {
                ProductId = y.Key,
                CountOfProducts = y.Count()
            }).ToList();                        
            ViewBag.CountProduct = countProduct.Count;

            var countPurchaseRequest = _applicationDbContext.PurchaseRequests.GroupBy(u => u.PurchaseRequestId).Select(y => new
            {
                PurchaseRequestId = y.Key,
                CountOfPurchaseRequests= y.Count()
            }).ToList();
            ViewBag.CountPurchaseRequest = countPurchaseRequest.Count;

            return View();
        }

        [HttpPost]
        public IActionResult Json(UserActiveViewModel viewModel, ProductViewModel productView)
        {
            var jsondata = _userActiveRepository.GetAllUser().GroupBy(u => u.CreateDateTime.ToString("MMMM yyyy")).Select(y => new
            {
                UserActiveId = y.Key,
                CountOfUsers = y.Count()
            }).ToList();

            var barChartJsonData = _productRepository.GetAllProduct().GroupBy(u => u.PrincipalId).Select(y => new
            {
                ProductId = y.Key,
                CountOfProduct = y.Count()
            }).ToList();

            var arrayData = new
            {
                UserResult = jsondata,
                ProductResult = barChartJsonData
            };
            return Json(arrayData);
        }
    }
}
