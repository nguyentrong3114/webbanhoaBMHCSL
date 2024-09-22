
using BMHCSDL.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyMvcApi.Areas.User.Controllers{
[Area("User")]
[AuthorizeUser]
public class ManageProfileController : Controller
{
    public IActionResult ManageProfile()
    {
        return View();
    }
}
}