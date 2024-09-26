using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BMHCSDL.Filters
{
    public class AuthorizeUserAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var userRole = context.HttpContext.Session.GetString("UserRole");

            if (!string.Equals(userRole, "user_role", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult("Denied", "Account", null);
            }

            base.OnActionExecuting(context);
        }
    }
}
