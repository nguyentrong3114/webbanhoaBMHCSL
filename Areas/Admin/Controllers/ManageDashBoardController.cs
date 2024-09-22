
using BMHCSDL.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BMHCSDL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeSys]
    public class ManageDashBoardController : Controller
    {
        public IActionResult DeliveryDashBoard()
        {
            return View();
        }
        public IActionResult OverviewDashBoard()
        {
            return View();
        }
        public IActionResult CustomerDashBoard()
        {
            return View();
        }
    }
}