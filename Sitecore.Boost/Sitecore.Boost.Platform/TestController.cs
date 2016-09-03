using System.Web.Mvc;

namespace Sitecore.Boost.Platform
{
    public class TestController : Controller
    {
        public ActionResult GetView()
        {
            return View("/views/testview.cshtml");
        }
    }
}
