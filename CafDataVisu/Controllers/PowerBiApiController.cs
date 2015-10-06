using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CafDataVisu.Controllers
{
    public class PowerBiApiController : ApiController
    {
        public string Get(string tileId, string dashboardId, string groupId)
        {
            var baseAddress = new Uri("https://api.powerbi.com/");
            string tileUrl;

            //AuthenticationResult token = (HttpContext.Current.Session["authResult"]) as AuthenticationResult;
            string token = HttpContext.Current.Request.Cookies["Authorize"]?["access_token"];
            
            using (var httpClient = new HttpClient {BaseAddress = baseAddress})
            {
                httpClient.DefaultRequestHeaders.TryAddWithoutValidation("authorization", $"Bearer {token}");

                Task<HttpResponseMessage> t =
                    httpClient.GetAsync("beta/myorg/groups/" + groupId + "/dashboards/" + dashboardId + "/tiles/" +
                                        tileId);

                t.Wait();

                var response = t.Result;

                var t2 = response.Content.ReadAsStringAsync();

                t2.Wait();

                dynamic dyn = JObject.Parse(t2.Result);

                tileUrl = dyn.embedUrl;
            }
            return JsonConvert.SerializeObject(new {tileurl = tileUrl});
        }
    }
}