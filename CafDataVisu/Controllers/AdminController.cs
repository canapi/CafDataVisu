using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using CafDataVisu.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;

namespace CafDataVisu.Controllers
{
    public class AdminController : Controller
    {
        private readonly string clientId = "768ce687-cc5c-4ddb-8eac-8298edb354a3";
        private readonly string clientSecret = "ny94x+Uo9nf1XJnoGkyuZSe4MCTJbEIH02NF5PSmauc=";

        private readonly string powerbiapiUrl = "https://analysis.windows.net/powerbi/api";

        //Redirect uri must match the redirect_uri used when requesting Authorization code.
        private readonly string authorityUri = "https://login.windows.net/common/oauth2/authorize/";

        private readonly Uri _baseAddress = new Uri("https://api.powerbi.com/");

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

            return Redirect($"{authorityUri}?{queryString}");
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

            using (var httpClient = new HttpClient {BaseAddress = _baseAddress})
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization",
                    $"Bearer {HttpContext.Request.Cookies["Authorize"]?["access_token"]}");

                HttpResponseMessage response = httpClient.GetAsync($"beta/myorg/groups/{groupId}/dashboards").Result;
                string result = response.Content.ReadAsStringAsync().Result;

                dynamic dyn = JObject.Parse(result);

                for (int i = 0; i < dyn.value.Count; i++)
                {
                    var newD = new DashBoard
                    {
                        GroupId = groupId,
                        GroupName = groupName,
                        Id = dyn.value[i].id,
                        Name = dyn.value[i].displayName,
                        Tiles = ListTilesForDashboard(dyn.value[i].id.ToString(), groupId)
                    };
                    res.Add(newD);
                }
            }
            return res;
        }

        private List<Tile> ListTilesForDashboard(string dashBoardId, string groupId)
        {
            List<Tile> res = new List<Tile>();

            using (var httpClient = new HttpClient {BaseAddress = _baseAddress})
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization",
                    $"Bearer {HttpContext.Request.Cookies["Authorize"]?["access_token"]}");

                HttpResponseMessage response =
                    httpClient.GetAsync($"beta/myorg/groups/{groupId}/dashboards/{dashBoardId}/tiles").Result;

                string result = response.Content.ReadAsStringAsync().Result;

                dynamic dyn = JObject.Parse(result);

                for (int i = 0; i < dyn.value.Count; i++)
                {
                    res.Add(
                        new Tile
                        {
                            Id = dyn.value[i].id,
                            Name = dyn.value[i].title
                        });
                }
            }
            return res;
        }

        private string GetGroupId(string groupName)
        {
            string groupId = string.Empty;

            using (var httpClient = new HttpClient {BaseAddress = _baseAddress})
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization",
                    $"Bearer {HttpContext.Request.Cookies["Authorize"]?["access_token"]}");

                HttpResponseMessage response = httpClient.GetAsync("v1.0/myorg/groups").Result;

                string result = response.Content.ReadAsStringAsync().Result;

                dynamic dyn = JObject.Parse(result);

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
            TokenCache tokenCache = new TokenCache();

            AuthenticationContext authenticationContext = new AuthenticationContext(authorityUri, tokenCache);
            ClientCredential clientCredential = new ClientCredential
                (
                clientId,
                clientSecret
                );

            AuthenticationResult authenticationResult;
            try
            {
                authenticationResult = authenticationContext.AcquireTokenByAuthorizationCode(code,
                    new Uri(responseUri), clientCredential);
            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
            }

            if (authenticationResult == null)
                return new HttpStatusCodeResult(HttpStatusCode.Unauthorized);

            //Set Session "authResult" index string to the AuthenticationResult
            HttpCookie authCookie = new HttpCookie("Authorize")
            {
                Expires = authenticationResult.ExpiresOn.LocalDateTime,
                ["access_token"] = authenticationResult.AccessToken
            };
            HttpContext.Response.Cookies.Add(authCookie);

            //Redirect back to Home
            //return RedirectToAction("ListTiles", new {groupName = "Hackaton - Scop it"});
            return RedirectToAction("Index", "Home");
        }

        private string GetResponseUri()
        {
            var responseUri = "http://" + HttpContext.Request.Url.Host;
            if (responseUri == "http://localhost")
                responseUri += ":" + HttpContext.Request.Url.Port;

            responseUri += "/Admin/TreatResponse";

            return responseUri;
        }
    }
}