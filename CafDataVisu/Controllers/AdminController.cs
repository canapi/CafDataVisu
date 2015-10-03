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
using CafDataVisu.Models;
using System.Web.Caching;

namespace CafDataVisu.Controllers
{
    public class AdminController : Controller
    {
        string clientId = "768ce687-cc5c-4ddb-8eac-8298edb354a3";
        string clientSecret = "ny94x+Uo9nf1XJnoGkyuZSe4MCTJbEIH02NF5PSmauc=";

        string powerbiapiUrl = "https://analysis.windows.net/powerbi/api";

        //string responseUri = "http://localhost:44307/Admin/TreatResponse";

        //Redirect uri must match the redirect_uri used when requesting Authorization code.
        string authorityUri = "https://login.windows.net/common/oauth2/authorize/";

        Uri baseAddress = new Uri("https://api.powerbi.com/");

        public ActionResult Index()
        {
            var responseUri = GetResponseUri();

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
                {"resource", powerbiapiUrl},

                //After user authenticates, Azure AD will redirect back to the web app
                {"redirect_uri", responseUri}
             };

            //Create sign-in query string
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add(@params);

            //Redirect authority
            //Authority Uri is an Azure resource that takes a client id to get an Access token
            string authorityUri = "https://login.windows.net/common/oauth2/authorize/";

            return Redirect(String.Format("{0}?{1}", authorityUri, queryString));
        }

        public ActionResult ListTiles(string groupName)
        {
            List<DashBoard> dashboards = ListGroupDashboards(groupName);

            return View(dashboards);
        }

        private List<DashBoard> ListGroupDashboards(string groupName)
        {
            string groupId = GetGroupId(groupName);

            List<DashBoard> res = new List<DashBoard>();

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                AuthenticationResult token = (this.HttpContext.Cache["authResult"] ) as AuthenticationResult;

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + token.AccessToken);

                Task<HttpResponseMessage> t = httpClient.GetAsync("beta/myorg/groups/" + groupId + "/dashboards");

                t.Wait();

                var response = (HttpResponseMessage)(t.Result);

                var t2 = response.Content.ReadAsStringAsync();

                t2.Wait();

                dynamic dyn = JObject.Parse(t2.Result);

                for (int i = 0; i < dyn.value.Count; i++)
                {

                    var newD = new DashBoard()
                    {
                        GroupId = groupId,
                        GroupName = groupName,
                        Id = dyn.value[i].id,
                        Name = dyn.value[i].displayName
                    };

                    newD.Tiles = ListTilesForDashboard(dyn.value[i].id.ToString(), groupId);

                    res.Add(newD);
                }
            }
            return res;
        }

        private List<Tile> ListTilesForDashboard(string dashBoardId, string groupId)
        {
            List<Tile> res = new List<Tile>();

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                AuthenticationResult token = (this.HttpContext.Cache["authResult"] ) as AuthenticationResult;

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + token.AccessToken);

                Task<HttpResponseMessage> t = httpClient.GetAsync("beta/myorg/groups/"+groupId+"/dashboards/"+ dashBoardId + "/tiles");

                t.Wait();

                var response = (HttpResponseMessage)(t.Result);

                var t2 = response.Content.ReadAsStringAsync();

                t2.Wait();

                dynamic dyn = JObject.Parse(t2.Result);

                for (int i = 0; i < dyn.value.Count; i++)
                {
                    res.Add(new Tile()
                    {
                        Id = dyn.value[i].id,
                        Name = dyn.value[i].title,
                    }
                    );
                }
            }
            return res;
        }

        private string GetGroupId(string groupName)
        {
            string groupId = string.Empty;

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                AuthenticationResult token = (this.HttpContext.Cache["authResult"] ) as AuthenticationResult;

                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + token.AccessToken);

                Task<HttpResponseMessage> t = httpClient.GetAsync("v1.0/myorg/groups");

                t.Wait();

                var response = (HttpResponseMessage)(t.Result);

                var t2 = response.Content.ReadAsStringAsync();

                t2.Wait();

                dynamic dyn = JObject.Parse(t2.Result);

                for (int i = 0; i < dyn.value.Count; i++)
                {
                    if (dyn.value[i].name == groupName)
                        groupId = dyn.value[i].id;
                }
            }

            return groupId;
        }

        public ActionResult TreatResponse()
        {
            var responseUri = GetResponseUri();

            string code = Request["code"];

            // Get auth token from auth code       
            TokenCache TC = new TokenCache();

            AuthenticationContext AC = new AuthenticationContext(authorityUri, TC);
            ClientCredential cc = new ClientCredential
                (
                clientId,
                clientSecret
                );

            AuthenticationResult AR = AC.AcquireTokenByAuthorizationCode(code, new Uri(responseUri), cc);

            //Set Session "authResult" index string to the AuthenticationResult

            this.HttpContext.Cache["authResult"] = AR;


            //Redirect back to Default.aspx
            return RedirectToAction("ListTiles", new { groupName = "Hackaton - Scop it" });
        }

        private string GetResponseUri()
        {
            string responseUri;

            responseUri = "http://" +  HttpContext.Request.Url.Host;
            if (responseUri == "http://localhost")
                responseUri += ":" + HttpContext.Request.Url.Port;

            responseUri += "/Admin/TreatResponse";

            return responseUri;

        }
    }
}