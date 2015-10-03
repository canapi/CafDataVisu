using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Specialized;
using System.IO;
using Newtonsoft.Json;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace CafDataVisu.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
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
            using (StreamReader sr = new StreamReader(Server.MapPath("~/dataweb.json")))
            {
                ret = sr.ReadToEnd();
            }
            return Content(ret, "application/json");
        }
    }
}