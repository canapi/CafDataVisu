using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;


namespace CafDataVisu.Controllers
{
    public class PowerBiApiController : ApiController
    {
        public string Get(string tileId, string dashboardId, string groupId)
        {
            var baseAddress = new Uri("https://api.powerbi.com/");
            string tileUrl = "https://app.powerbi.com/embed?dashboardId=" + dashboardId + "&tileId=" + tileId;

            AuthenticationResult token = (HttpContext.Current.Cache["authResult"]) as AuthenticationResult;

            using (var httpClient = new HttpClient { BaseAddress = baseAddress })
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", "Bearer " + token.AccessToken);

                Task<HttpResponseMessage> t = httpClient.GetAsync("beta/myorg/groups/" + groupId + "/dashboards/"+dashboardId+"/tiles/" + tileId);

                t.Wait();

                var response = (HttpResponseMessage)(t.Result);

                var t2 = response.Content.ReadAsStringAsync();

                t2.Wait();

                dynamic dyn = JObject.Parse(t2.Result);

                tileUrl = dyn.embedUrl;

            }
            return JsonConvert.SerializeObject(new { tileurl = tileUrl, accessToken = token.AccessToken });
        }
    }
}