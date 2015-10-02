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
    public class AdminController : Controller
    {
        string clientId = "768ce687-cc5c-4ddb-8eac-8298edb354a3";
        string clientSecret = "ny94x+Uo9nf1XJnoGkyuZSe4MCTJbEIH02NF5PSmauc=";

        public ActionResult Index()
        {
            // Create a query string
            //Create a sign-in NameValueCollection for query string
            var @params = new NameValueCollection
             {
                //Azure AD will return an authorization code. 
                //See the Redirect class to see how "code" is used to AcquireTokenByAuthorizationCode
                {"response_type", "code"},

                //Client ID is used by the application to identify themselves to the users that they are requesting permissions from. 
                //You get the client id when you register your Azure app.
                {"client_id", clientId},

                //Resource uri to the Power BI resource to be authorized
                {"resource", "https://analysis.windows.net/powerbi/api"},

                //After user authenticates, Azure AD will redirect back to the web app
                {"redirect_uri", "http://localhost:44307/Admin/TreatResponse"}
             };

            //Create sign-in query string
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add(@params);

            //Redirect authority
            //Authority Uri is an Azure resource that takes a client id to get an Access token
            string authorityUri = "https://login.windows.net/common/oauth2/authorize/";

            return Redirect(String.Format("{0}?{1}", authorityUri, queryString));
        }

        public ActionResult ListGroup()
        {
            var baseAddress = new Uri("https://api.powerbi.com/");

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                AuthenticationResult token = (Session["authResult"]) as AuthenticationResult;

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + token.AccessToken);

                Task<HttpResponseMessage> t = httpClient.GetAsync("v1.0/myorg/groups");

                t.Wait();

                var response = (HttpResponseMessage)(t.Result);

                var t2 = response.Content.ReadAsStringAsync();

                t2.Wait();

                var content = (t2.Result).ToString();
            }

            return View();
        }

        public ActionResult GetTile()
        {

            //var baseAddress = new Uri("https://api.powerbi.com/");
            var baseAdressDebug = "http://private-anon-dd2e084e9-powerbi.apiary-proxy.com";
            string tileUrl = "https://app.powerbi.com/embed?dashboardId=0c0a46c6-31bd-4cdd-84ac-f6fb153d150b&tileId=780b9ae7-0fdb-4701-a549-188250b80ded";

            AuthenticationResult token = (Session["authResult"]) as AuthenticationResult;



            //string dashId = "0c0a46c6-31bd-4cdd-84ac-f6fb153d150b";

            //var action = "/beta/myorg/" + dashId + "/tiles";


            //using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            //{

            //    httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + token.AccessToken);

            //    Task<HttpResponseMessage> t = httpClient.GetAsync(action);

            //    t.Wait();

            //    var response = (HttpResponseMessage)(t.Result);

            //    var t2 = response.Content.ReadAsStringAsync();

            //    t2.Wait();

            //    var content = (t2.Result).ToString();

            //    dynamic dyn = JObject.Parse(content);

            //    tileUrl = dyn.Values[0].EmbedUrl;
            //}

            ViewBag.TileUrl = tileUrl;


            ViewBag.Token = token.AccessToken;

            return View();
        }

        public ActionResult TreatResponse()
        {
            //Redirect uri must match the redirect_uri used when requesting Authorization code.
            string redirectUri = "http://localhost:44307/Admin/TreatResponse";
            string authorityUri = "https://login.windows.net/common/oauth2/authorize/";

            string code = Request["code"];

            // Get auth token from auth code       
            TokenCache TC = new TokenCache();

            AuthenticationContext AC = new AuthenticationContext(authorityUri, TC);
            ClientCredential cc = new ClientCredential
                (
                clientId,
                clientSecret
                );

            AuthenticationResult AR = AC.AcquireTokenByAuthorizationCode(code, new Uri(redirectUri), cc);

            //Set Session "authResult" index string to the AuthenticationResult
            Session["authResult"] = AR;

            //Redirect back to Default.aspx
            return RedirectToAction("ListGroup");
        }
    }
}