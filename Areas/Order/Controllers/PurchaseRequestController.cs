﻿using FastReport.Data;
using FastReport.Export.PdfSimple;
using FastReport.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PurchasingSystemApps.Areas.MasterData.Repositories;
using PurchasingSystemApps.Areas.Order.Models;
using PurchasingSystemApps.Areas.Order.Repositories;
using PurchasingSystemApps.Areas.Order.ViewModels;
using PurchasingSystemApps.Data;
using PurchasingSystemApps.Models;
using PurchasingSystemApps.Repositories;
using System.Data;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace PurchasingSystemApps.Areas.Order.Controllers
{
    [Area("Order")]
    [Route("Order/[Controller]/[Action]")]
    public class PurchaseRequestController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IPurchaseRequestRepository _purchaseRequestRepository;
        private readonly IUserActiveRepository _userActiveRepository;
        private readonly IProductRepository _productRepository;
        private readonly ITermOfPaymentRepository _termOfPaymentRepository;
        private readonly IApprovalRepository _approvalRepository;
        private readonly IPurchaseOrderRepository _purchaseOrderRepository;
        private readonly IDueDateRepository _dueDateRepository;

        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;


        public PurchaseRequestController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext applicationDbContext,
            IPurchaseRequestRepository purchaseRequestRepository,
            IUserActiveRepository userActiveRepository,
            IProductRepository productRepository,
            ITermOfPaymentRepository termOfPaymentRepository,
            IApprovalRepository approvalRepository,
            IPurchaseOrderRepository purchaseOrderRepository,
            IDueDateRepository dueDateRepository,

            IHostingEnvironment hostingEnvironment,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration
        )
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _applicationDbContext = applicationDbContext;
            _purchaseRequestRepository = purchaseRequestRepository;
            _userActiveRepository = userActiveRepository;
            _productRepository = productRepository;
            _termOfPaymentRepository = termOfPaymentRepository;
            _approvalRepository = approvalRepository;
            _purchaseOrderRepository = purchaseOrderRepository;
            _dueDateRepository = dueDateRepository;
            
            _hostingEnvironment = hostingEnvironment;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
        }

        public JsonResult LoadProduk(Guid Id)
        {
            var produk = _applicationDbContext.Products.Include(p => p.Principal).Include(s => s.Measurement).Include(d => d.Discount).Where(p => p.ProductId == Id).FirstOrDefault();
            return new JsonResult(produk);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            ViewBag.Active = "PurchaseRequest";

            var countPurchaseRequest = _applicationDbContext.PurchaseRequests.Where(p => p.Status == "Not Approved").GroupBy(u => u.PurchaseRequestId).Select(y => new
            {
                PurchaseRequestId = y.Key,
                CountOfPurchaseRequests = y.Count()
            }).ToList();
            ViewBag.CountPurchaseRequest = countPurchaseRequest.Count;

            var countApproval = _applicationDbContext.Approvals.Where(p => p.Status == "Not Approved").GroupBy(u => u.PurchaseRequestId).Select(y => new
            {
                ApprovalId = y.Key,
                CountOfApprovals = y.Count()
            }).ToList();
            ViewBag.CountApproval = countApproval.Count;

            var data = _purchaseRequestRepository.GetAllPurchaseRequest();
            return View(data);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Index(DateTime tglAwalPencarian, DateTime tglAkhirPencarian)
        {
            ViewBag.Active = "PurchaseRequest";
            ViewBag.tglAwalPencarian = tglAwalPencarian.ToString("dd MMMM yyyy");
            ViewBag.tglAkhirPencarian = tglAkhirPencarian.ToString("dd MMMM yyyy");

            var data = _purchaseRequestRepository.GetAllPurchaseRequest().Where(r => r.CreateDateTime.Date >= tglAwalPencarian && r.CreateDateTime.Date <= tglAkhirPencarian).ToList();
            return View(data);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePurchaseRequest()
        {
            ViewBag.Active = "PurchaseRequest";

            _signInManager.IsSignedIn(User);
            var getUser = _userActiveRepository.GetAllUserLogin().Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            ViewBag.Product = new SelectList(await _productRepository.GetProducts(), "ProductId", "ProductName", SortOrder.Ascending);
            ViewBag.Approval = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);
            ViewBag.TermOfPayment = new SelectList(await _termOfPaymentRepository.GetTermOfPayments(), "TermOfPaymentId", "TermOfPaymentName", SortOrder.Ascending);
            ViewBag.DueDate = new SelectList(await _dueDateRepository.GetDueDates(), "DueDateId", "Value", SortOrder.Ascending);

            PurchaseRequest purchaseRequest = new PurchaseRequest()
            {
                UserAccessId = getUser.Id,
            };
            purchaseRequest.PurchaseRequestDetails.Add(new PurchaseRequestDetail() { PurchaseRequestDetailId = Guid.NewGuid() });

            var dateNow = DateTimeOffset.Now;
            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            var lastCode = _purchaseRequestRepository.GetAllPurchaseRequest().Where(d => d.CreateDateTime.ToString("yyMMdd") == dateNow.ToString("yyMMdd")).OrderByDescending(k => k.PurchaseRequestNumber).FirstOrDefault();
            if (lastCode == null)
            {
                purchaseRequest.PurchaseRequestNumber = "PR" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.PurchaseRequestNumber.Substring(2, 6);

                if (lastCodeTrim != setDateNow)
                {
                    purchaseRequest.PurchaseRequestNumber = "PR" + setDateNow + "0001";
                }
                else
                {
                    purchaseRequest.PurchaseRequestNumber = "PR" + setDateNow + (Convert.ToInt32(lastCode.PurchaseRequestNumber.Substring(9, lastCode.PurchaseRequestNumber.Length - 9)) + 1).ToString("D4");
                }
            }

            return View(purchaseRequest);
        }


        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePurchaseRequest(PurchaseRequest model)
        {
            ViewBag.Active = "PurchaseRequest";

            var dateNow = DateTimeOffset.Now;
            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            var lastCode = _purchaseRequestRepository.GetAllPurchaseRequest().Where(d => d.CreateDateTime.ToString("yyMMdd") == dateNow.ToString("yyMMdd")).OrderByDescending(k => k.PurchaseRequestNumber).FirstOrDefault();
            if (lastCode == null)
            {
                model.PurchaseRequestNumber = "PR" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.PurchaseRequestNumber.Substring(2, 6);

                if (lastCodeTrim != setDateNow)
                {
                    model.PurchaseRequestNumber = "PR" + setDateNow + "0001";
                }
                else
                {
                    model.PurchaseRequestNumber = "PR" + setDateNow + (Convert.ToInt32(lastCode.PurchaseRequestNumber.Substring(9, lastCode.PurchaseRequestNumber.Length - 9)) + 1).ToString("D4");
                }
            }

            var getUser = _userActiveRepository.GetAllUserLogin().Where(u => u.UserName == User.Identity.Name).FirstOrDefault();            

            if (ModelState.IsValid)
            {
                var purchaseRequest = new PurchaseRequest
                {
                    CreateDateTime = DateTime.Now,
                    CreateBy = new Guid(getUser.Id), //Convert Guid to String
                    PurchaseRequestId = model.PurchaseRequestId,
                    PurchaseRequestNumber = model.PurchaseRequestNumber,
                    UserAccessId = getUser.Id,
                    UserApprove1Id = model.UserApprove1Id,
                    UserApprove2Id = model.UserApprove2Id,
                    UserApprove3Id = model.UserApprove3Id,
                    TermOfPaymentId = model.TermOfPaymentId,
                    Status = model.Status,
                    QtyTotal = model.QtyTotal,
                    GrandTotal = Math.Truncate(model.GrandTotal),
                    DueDateId = model.DueDateId,
                    Note = model.Note,
                };

                var ItemsList = new List<PurchaseRequestDetail>();

                foreach (var item in model.PurchaseRequestDetails)
                {
                    ItemsList.Add(new PurchaseRequestDetail
                    {
                        CreateDateTime = DateTime.Now,
                        CreateBy = new Guid(getUser.Id),
                        ProductNumber = item.ProductNumber,
                        ProductName = item.ProductName,
                        Measurement = item.Measurement,
                        Principal = item.Principal,                        
                        Qty = item.Qty,
                        Price = Math.Truncate(item.Price),
                        Discount = item.Discount,
                        SubTotal = Math.Truncate(item.SubTotal)
                    });
                }

                purchaseRequest.PurchaseRequestDetails = ItemsList;
                _purchaseRequestRepository.Tambah(purchaseRequest);

                var approval = new Approval
                {
                    CreateDateTime = DateTime.Now,
                    CreateBy = new Guid(getUser.Id),
                    PurchaseRequestId = purchaseRequest.PurchaseRequestId,
                    PurchaseRequestNumber = purchaseRequest.PurchaseRequestNumber,
                    UserAccessId = getUser.Id.ToString(),
                    UserApprove1Id = purchaseRequest.UserApprove1Id,
                    ApproveByUser1 = purchaseRequest.UserApprove1.FullName,
                    User1ApproveTime = "",
                    User1ApproveDate = DateTime.MinValue,
                    UserApprove2Id = purchaseRequest.UserApprove2Id,
                    ApproveByUser2 = purchaseRequest.UserApprove2.FullName,
                    User2ApproveTime = "",
                    User2ApproveDate = DateTime.MinValue,
                    UserApprove3Id = purchaseRequest.UserApprove3Id,
                    ApproveByUser3 = purchaseRequest.UserApprove3.FullName,
                    User3ApproveTime = "",
                    User3ApproveDate = DateTime.MinValue,
                    //ApproveBy = "",
                    Status = purchaseRequest.Status,
                    Note = purchaseRequest.Note,
                };
                _approvalRepository.Tambah(approval);

                TempData["SuccessMessage"] = "Number " + model.PurchaseRequestNumber + " Saved";
                return Json(new { redirectToUrl = Url.Action("Index", "PurchaseRequest") });
            }
            else
            {
                ViewBag.Product = new SelectList(await _productRepository.GetProducts(), "ProductId", "ProductName", SortOrder.Ascending);
                ViewBag.Approval = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);
                ViewBag.TermOfPayment = new SelectList(await _termOfPaymentRepository.GetTermOfPayments(), "TermOfPaymentId", "TermOfPaymentName", SortOrder.Ascending);
                ViewBag.DueDate = new SelectList(await _dueDateRepository.GetDueDates(), "DueDateId", "Value", SortOrder.Ascending);
                TempData["WarningMessage"] = "Please, input all data !";
                return View(model);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> DetailPurchaseRequest(Guid Id)
        {
            ViewBag.Active = "PurchaseRequest";

            ViewBag.Product = new SelectList(await _productRepository.GetProducts(), "ProductId", "ProductName", SortOrder.Ascending);
            ViewBag.Approval = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);
            ViewBag.TermOfPayment = new SelectList(await _termOfPaymentRepository.GetTermOfPayments(), "TermOfPaymentId", "TermOfPaymentName", SortOrder.Ascending);
            ViewBag.DueDate = new SelectList(await _dueDateRepository.GetDueDates(), "DueDateId", "Value", SortOrder.Ascending);

            var purchaseRequest = await _purchaseRequestRepository.GetPurchaseRequestById(Id);

            if (purchaseRequest == null)
            {
                Response.StatusCode = 404;
                return View("PurchaseRequestNotFound", Id);
            }

            PurchaseRequest model = new PurchaseRequest
            {
                PurchaseRequestId = purchaseRequest.PurchaseRequestId,
                PurchaseRequestNumber = purchaseRequest.PurchaseRequestNumber,
                UserAccessId = purchaseRequest.UserAccessId,
                UserApprove1Id = purchaseRequest.UserApprove1Id,
                UserApprove2Id = purchaseRequest.UserApprove2Id,
                UserApprove3Id = purchaseRequest.UserApprove3Id,
                TermOfPaymentId = purchaseRequest.TermOfPaymentId,
                Status = purchaseRequest.Status,
                QtyTotal = purchaseRequest.QtyTotal,
                GrandTotal = Math.Truncate(purchaseRequest.GrandTotal),
                DueDateId = purchaseRequest.DueDateId,
                Note = purchaseRequest.Note,
            };

            var ItemsList = new List<PurchaseRequestDetail>();

            foreach (var item in purchaseRequest.PurchaseRequestDetails)
            {
                ItemsList.Add(new PurchaseRequestDetail
                {
                    ProductNumber = item.ProductNumber,
                    ProductName = item.ProductName,
                    Measurement = item.Measurement,
                    Principal = item.Principal,                    
                    Qty = item.Qty,
                    Price = Math.Truncate(item.Price),
                    Discount = item.Discount,
                    SubTotal = Math.Truncate(item.SubTotal)
                });
            }

            model.PurchaseRequestDetails = ItemsList;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> DetailPurchaseRequest(PurchaseRequest model)
        {
            ViewBag.Active = "PurchaseRequest";

            var getUser = _userActiveRepository.GetAllUserLogin().Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            if (ModelState.IsValid)
            {
                PurchaseRequest purchaseRequest = _purchaseRequestRepository.GetAllPurchaseRequest().Where(p => p.PurchaseRequestNumber == model.PurchaseRequestNumber).FirstOrDefault();
                Approval approval = _approvalRepository.GetAllApproval().Where(p => p.PurchaseRequestNumber == model.PurchaseRequestNumber).FirstOrDefault();

                if (purchaseRequest != null)
                {
                    if (approval != null)
                    {
                        purchaseRequest.UpdateDateTime = DateTime.Now;
                        purchaseRequest.UpdateBy = new Guid(getUser.Id);                        
                        purchaseRequest.UserApprove1Id = model.UserApprove1Id;
                        purchaseRequest.UserApprove2Id = model.UserApprove2Id;
                        purchaseRequest.UserApprove3Id = model.UserApprove3Id;
                        purchaseRequest.TermOfPaymentId = model.TermOfPaymentId;                                              
                        purchaseRequest.QtyTotal = model.QtyTotal;
                        purchaseRequest.GrandTotal = model.GrandTotal;
                        purchaseRequest.DueDateId = model.DueDateId;
                        purchaseRequest.Note = model.Note;
                        purchaseRequest.PurchaseRequestDetails = model.PurchaseRequestDetails;

                        approval.User1ApproveDate = DateTime.MinValue;
                        approval.UserApprove1Id = model.UserApprove1Id;
                        approval.ApproveByUser1 = model.UserApprove1.FullName;
                        approval.User2ApproveDate = DateTime.MinValue;
                        approval.UserApprove2Id = model.UserApprove2Id;
                        approval.ApproveByUser2 = model.UserApprove2.FullName;
                        approval.User3ApproveDate = DateTime.MinValue;
                        approval.UserApprove3Id = model.UserApprove3Id;
                        approval.ApproveByUser3 = model.UserApprove3.FullName;
                        approval.Note = model.Note;

                        _applicationDbContext.Entry(approval).State = EntityState.Modified;
                        _purchaseRequestRepository.Update(purchaseRequest);

                        TempData["SuccessMessage"] = "Number " + model.PurchaseRequestNumber + " Changes saved";
                        return Json(new { redirectToUrl = Url.Action("Index", "PurchaseRequest") });
                    }
                    else
                    {
                        ViewBag.Product = new SelectList(await _productRepository.GetProducts(), "ProductId", "ProductName", SortOrder.Ascending);
                        ViewBag.Approval = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);
                        ViewBag.TermOfPayment = new SelectList(await _termOfPaymentRepository.GetTermOfPayments(), "TermOfPaymentId", "TermOfPaymentName", SortOrder.Ascending);
                        ViewBag.DueDate = new SelectList(await _dueDateRepository.GetDueDates(), "DueDateId", "Value", SortOrder.Ascending);
                        TempData["WarningMessage"] = "Number " + model.PurchaseRequestNumber + " Not Found !!!";
                        return View(model);
                    }
                }
                else
                {
                    ViewBag.Product = new SelectList(await _productRepository.GetProducts(), "ProductId", "ProductName", SortOrder.Ascending);
                    ViewBag.Approval = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);
                    ViewBag.TermOfPayment = new SelectList(await _termOfPaymentRepository.GetTermOfPayments(), "TermOfPaymentId", "TermOfPaymentName", SortOrder.Ascending);
                    ViewBag.DueDate = new SelectList(await _dueDateRepository.GetDueDates(), "DueDateId", "Value", SortOrder.Ascending);
                    TempData["WarningMessage"] = "Number " + model.PurchaseRequestNumber + " Already exists !!!";
                    return View(model);
                }
            }
            ViewBag.Product = new SelectList(await _productRepository.GetProducts(), "ProductId", "ProductName", SortOrder.Ascending);
            ViewBag.Approval = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);
            ViewBag.TermOfPayment = new SelectList(await _termOfPaymentRepository.GetTermOfPayments(), "TermOfPaymentId", "TermOfPaymentName", SortOrder.Ascending);
            ViewBag.DueDate = new SelectList(await _dueDateRepository.GetDueDates(), "DueDateId", "Value", SortOrder.Ascending);
            TempData["WarningMessage"] = "Number " + model.PurchaseRequestNumber + " Failed saved";
            return Json(new { redirectToUrl = Url.Action("Index", "PurchaseRequest") });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GeneratePurchaseOrder(Guid Id)
        {
            ViewBag.Active = "PurchaseRequest";

            ViewBag.Product = new SelectList(await _productRepository.GetProducts(), "ProductId", "ProductName", SortOrder.Ascending);
            ViewBag.Approval = new SelectList(await _userActiveRepository.GetUserActives(), "UserActiveId", "FullName", SortOrder.Ascending);
            ViewBag.TermOfPayment = new SelectList(await _termOfPaymentRepository.GetTermOfPayments(), "TermOfPaymentId", "TermOfPaymentName", SortOrder.Ascending);
            ViewBag.DueDate = new SelectList(await _dueDateRepository.GetDueDates(), "DueDateId", "Value", SortOrder.Ascending);

            PurchaseRequest purchaseRequest = _applicationDbContext.PurchaseRequests
                .Include(d => d.PurchaseRequestDetails)
                .Include(u => u.ApplicationUser)
                .Include(a1 => a1.UserApprove1)
                .Include(a2 => a2.UserApprove2)
                .Include(a3 => a3.UserApprove3)
                .Include(p => p.TermOfPayment)
                .Include(e => e.DueDate)
                .Where(p => p.PurchaseRequestId == Id).FirstOrDefault();

            _signInManager.IsSignedIn(User);

            var getUser = _userActiveRepository.GetAllUserLogin().Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            PurchaseOrder po = new PurchaseOrder();            
            
            var dateNow = DateTimeOffset.Now;
            var setDateNow = DateTimeOffset.Now.ToString("yyMMdd");

            var lastCode = _purchaseOrderRepository.GetAllPurchaseOrder().Where(d => d.CreateDateTime.ToString("yyMMdd") == dateNow.ToString("yyMMdd")).OrderByDescending(k => k.PurchaseOrderNumber).FirstOrDefault();
            if (lastCode == null)
            {
                po.PurchaseOrderNumber = "PO" + setDateNow + "0001";
            }
            else
            {
                var lastCodeTrim = lastCode.PurchaseOrderNumber.Substring(2, 6);

                if (lastCodeTrim != setDateNow)
                {
                    po.PurchaseOrderNumber = "PO" + setDateNow + "0001";
                }
                else
                {
                    po.PurchaseOrderNumber = "PO" + setDateNow + (Convert.ToInt32(lastCode.PurchaseOrderNumber.Substring(9, lastCode.PurchaseOrderNumber.Length - 9)) + 1).ToString("D4");
                }
            }

            ViewBag.PurchaseOrderNumber = po.PurchaseOrderNumber;

            var getPr = new PurchaseRequest()
            {
                PurchaseRequestId = purchaseRequest.PurchaseRequestId,
                PurchaseRequestNumber = purchaseRequest.PurchaseRequestNumber,
                UserAccessId = purchaseRequest.UserAccessId,
                UserApprove1Id = purchaseRequest.UserApprove1Id,
                UserApprove2Id = purchaseRequest.UserApprove2Id,
                UserApprove3Id = purchaseRequest.UserApprove3Id,
                TermOfPaymentId = purchaseRequest.TermOfPaymentId,
                Status = purchaseRequest.Status,
                QtyTotal = purchaseRequest.QtyTotal,
                GrandTotal = Math.Truncate(purchaseRequest.GrandTotal),
                DueDateId = purchaseRequest.DueDateId,
                Note = purchaseRequest.Note
            };

            var ItemsList = new List<PurchaseRequestDetail>();

            foreach (var item in purchaseRequest.PurchaseRequestDetails)
            {
                ItemsList.Add(new PurchaseRequestDetail
                {
                    CreateDateTime = DateTime.Now,
                    CreateBy = new Guid(getUser.Id),
                    ProductNumber = item.ProductNumber,
                    ProductName = item.ProductName,
                    Principal = item.Principal,
                    Measurement = item.Measurement,
                    Qty = item.Qty,
                    Price = Math.Truncate(item.Price),
                    Discount = item.Discount,
                    SubTotal = Math.Truncate(item.SubTotal)
                });
            }

            getPr.PurchaseRequestDetails = ItemsList;          
            return View(getPr);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GeneratePurchaseOrder(PurchaseRequest model)
        {
            ViewBag.Active = "PurchaseRequest";

            PurchaseRequest purchaseRequest = await _purchaseRequestRepository.GetPurchaseRequestByIdNoTracking(model.PurchaseRequestId);

            _signInManager.IsSignedIn(User);

            var getUser = _userActiveRepository.GetAllUserLogin().Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            string getPurchaseOrderNumber = Request.Form["PONumber"];

            var updatePurchaseRequest = _purchaseRequestRepository.GetAllPurchaseRequest().Where(c => c.PurchaseRequestId == model.PurchaseRequestId).FirstOrDefault();
            if (updatePurchaseRequest != null)
            {
                {
                    updatePurchaseRequest.Status = getPurchaseOrderNumber;
                };
                _applicationDbContext.Entry(updatePurchaseRequest).State = EntityState.Modified;
            }

            var newPurchaseOrder = new PurchaseOrder
            {
                CreateDateTime = DateTime.Now,
                CreateBy = new Guid(getUser.Id),
                PurchaseRequestId = purchaseRequest.PurchaseRequestId,
                PurchaseRequestNumber = purchaseRequest.PurchaseRequestNumber,
                UserAccessId = getUser.Id.ToString(),
                UserApprove1Id = purchaseRequest.UserApprove1Id,
                UserApprove2Id = purchaseRequest.UserApprove2Id,
                UserApprove3Id = purchaseRequest.UserApprove3Id,
                TermOfPaymentId = purchaseRequest.TermOfPaymentId,
                Status = "InProcess",
                QtyTotal = purchaseRequest.QtyTotal,
                GrandTotal = Math.Truncate(purchaseRequest.GrandTotal),
                DueDateId = purchaseRequest.DueDateId,
                Note = purchaseRequest.Note
            };

            newPurchaseOrder.PurchaseOrderNumber = getPurchaseOrderNumber;

            var ItemsList = new List<PurchaseOrderDetail>();

            foreach (var item in purchaseRequest.PurchaseRequestDetails)
            {
                ItemsList.Add(new PurchaseOrderDetail
                {
                    CreateDateTime = DateTime.Now,
                    CreateBy = new Guid(getUser.Id),
                    ProductNumber = item.ProductNumber,
                    ProductName = item.ProductName,
                    Principal = item.Principal,
                    Measurement = item.Measurement,
                    Qty = item.Qty,
                    Price = Math.Truncate(item.Price),
                    Discount = item.Discount,
                    SubTotal = Math.Truncate(item.SubTotal)
                });
            }

            newPurchaseOrder.PurchaseOrderDetails = ItemsList;

            _purchaseOrderRepository.Tambah(newPurchaseOrder);

            TempData["SuccessMessage"] = "Number " + newPurchaseOrder.PurchaseOrderNumber + " Saved";
            return RedirectToAction("Index", "PurchaseRequest");
        }

        public async Task<IActionResult> PrintPurchaseRequest(Guid Id)
        {
            var purchaseRequest = await _purchaseRequestRepository.GetPurchaseRequestById(Id);

            var CreateDate = DateTime.Now.ToString("dd MMMM yyyy");
            var PrNumber = purchaseRequest.PurchaseRequestNumber;
            var CreateBy = purchaseRequest.ApplicationUser.NamaUser;
            var UserApprove1 = purchaseRequest.UserApprove1.FullName;
            var UserApprove2 = purchaseRequest.UserApprove2.FullName;
            var UserApprove3 = purchaseRequest.UserApprove3.FullName;
            var TermOfPayment = purchaseRequest.TermOfPayment.TermOfPaymentName;
            var DueDate = purchaseRequest.DueDate;
            var Note = purchaseRequest.Note;
            var GrandTotal = purchaseRequest.GrandTotal;
            var Tax = (GrandTotal / 100) * 11;
            var GrandTotalAfterTax = (GrandTotal + Tax);

            WebReport web = new WebReport();
            var path = $"{_webHostEnvironment.WebRootPath}\\Reporting\\PurchaseRequest.frx";
            web.Report.Load(path);

            var msSqlDataConnection = new MsSqlDataConnection();
            msSqlDataConnection.ConnectionString = _configuration.GetConnectionString("DefaultConnection");
            var Conn = msSqlDataConnection.ConnectionString;

            web.Report.SetParameterValue("Conn", Conn);
            web.Report.SetParameterValue("PurchaseRequestId", Id.ToString());
            web.Report.SetParameterValue("PrNumber", PrNumber);
            web.Report.SetParameterValue("CreateDate", CreateDate);
            web.Report.SetParameterValue("CreateBy", CreateBy);
            web.Report.SetParameterValue("UserApprove1", UserApprove1);
            web.Report.SetParameterValue("UserApprove2", UserApprove2);
            web.Report.SetParameterValue("UserApprove3", UserApprove3);
            web.Report.SetParameterValue("TermOfPayment", TermOfPayment);
            web.Report.SetParameterValue("DueDate", DueDate);
            web.Report.SetParameterValue("Note", Note);
            web.Report.SetParameterValue("GrandTotal", GrandTotal);
            web.Report.SetParameterValue("Tax", Tax);
            web.Report.SetParameterValue("GrandTotalAfterTax", GrandTotalAfterTax);

            web.Report.Prepare();
            Stream stream = new MemoryStream();
            web.Report.Export(new PDFSimpleExport(), stream);
            stream.Position = 0;
            return File(stream, "application/zip", (PrNumber + ".pdf"));
        }
    }
}
