using System.IO;
using System.Web.Mvc;

namespace CafDataVisu.Controllers
{

    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string token = HttpContext.Request.Cookies["Authorize"]?["access_token"];
            if (token == null)
                return RedirectToAction("Index", "Admin");
            return View();
        }

        public ActionResult Parcours()
        {
            return View();
        }

        public ActionResult ParcoursWeb()
        {
            return View();
        }

        public ContentResult GetData()
        {
            string ret;
            using (StreamReader sr = new StreamReader(Server.MapPath("~/data.json")))
            {
                ret = sr.ReadToEnd();
            }
            return Content(ret, "application/json");
        }

        public ContentResult GetDataWeb()
        {
            string ret;
            using (StreamReader sr = new StreamReader(Server.MapPath("~/dataweb.csv")))
            {
                ret = sr.ReadToEnd();
            }
            return Content(ret, "text/csv");
        }
    }
}