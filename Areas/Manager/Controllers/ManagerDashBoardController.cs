
using BMHCSDL.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BMHCSDL.Areas.Admin.Controllers
{
    [Area("Manager")]
    [AuthorizeManager]
    public class ManagerDashBoardController : Controller
    {
        public IActionResult OverviewDashBoard()
        {
            return View();
        }
        public IActionResult ManageProductDashBoard(){
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
    }
}