using System.Web.Mvc;

namespace MusicStuff.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AudioTest()
        {
            return View();
        }
    }
}