using Microsoft.AspNetCore.Mvc;

namespace TaskJect.Web.Controllers
{
    [Route("error")]
    public class ErrorController : Controller
    {

        [Route("404")]
        public IActionResult PageNotFound()
        {
            string originalPath = "unknown";
            if (HttpContext.Items.ContainsKey("originalPath"))
            {
                originalPath = HttpContext.Items["originalPath"] as string;
            }
            return View();
        }

        public IActionResult PageNotAccess() 
        { 
            return View();
        }
    }
}
