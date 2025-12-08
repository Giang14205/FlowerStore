using FlowerShop.Models.Vnpay;
using FlowerShop.Services.Vnpay;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        public PaymentController(IVnPayService vnPayService)
        {

            _vnPayService = vnPayService;
        }
        [HttpPost]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }
        
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult PaymentCallBack()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            return Json(response);
        }
        public IActionResult PaymentCallbackVnpay()
        {
            // Cần inject QlbhtContext vào PaymentController để cập nhật DB
            var response = _vnPayService.PaymentExecute(Request.Query);

            // ... [Logic tìm đơn hàng và cập nhật OrderStatusId theo response.VnPayResponseCode] ...

            // Sau đó Redirect về trang chi tiết đơn hàng (DetailSuccess) trong ShoppingCartController
            return RedirectToAction("DetailSuccess", "ShoppingCart", new { id = response.OrderId });
        }

    }
}
