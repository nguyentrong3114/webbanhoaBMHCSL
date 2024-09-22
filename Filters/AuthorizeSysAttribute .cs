using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BMHCSDL.Filters
{
    public class AuthorizeSysAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userName = context.HttpContext.Session.GetString("UserName");

            if (!string.Equals(userName, "sys", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Denied", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
