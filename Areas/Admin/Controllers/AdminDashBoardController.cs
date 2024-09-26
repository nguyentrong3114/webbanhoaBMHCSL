
using BMHCSDL.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BMHCSDL.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AuthorizeAdmin]
    public class AdminDashBoardController : Controller
    {

        public IActionResult OverviewDashBoard()
        {
            return View();
        }
        public IActionResult DeliveryDashBoard()
        {
            return View();
        }
        public IActionResult CustomerDashBoard()
        {
            return View();
        }
        public IActionResult RoleDashBoard()
        {
            return View();
        }
    }
}