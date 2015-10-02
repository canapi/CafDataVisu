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
    }
}