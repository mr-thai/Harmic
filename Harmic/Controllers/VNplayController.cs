using Microsoft.AspNetCore.Mvc;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

namespace Harmic.Controllers
{
    public class VNplayController : Controller
    {
        private readonly IVnpay _vnpay;
        public VNplayController(IVnpay vnpay) => _vnpay = vnpay;

        [HttpGet]
        public IActionResult CreatePaymentUrl(double moneyToPay, string description)
        {
            try
            {
                var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
                var request = new PaymentRequest
                {
                    PaymentId = DateTime.Now.Ticks,
                    Money = moneyToPay,
                    Description = description,
                    IpAddress = ipAddress,
                    BankCode = BankCode.ANY,
                    CreatedDate = DateTime.Now,
                    Currency = Currency.VND,
                    Language = DisplayLanguage.Vietnamese
                };
                var paymentUrl = _vnpay.GetPaymentUrl(request);
                return Redirect(paymentUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Callback URL configured in appsettings.json (ReturnUrl)
        [HttpGet]
        public IActionResult Callback()
        {
            var result = _vnpay.GetPaymentResult(Request.Query);
            // TODO: handle success/failure (save order, update status, etc.)
            return Ok(result);
        }
    }
}
