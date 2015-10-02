using Newtonsoft.Json;
using System.Web.Http;
using System.Web.Mvc;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Collections.Specialized;
using System.Web;

namespace CafDataScopItApi.Controllers
{
    public class DashboardsController : ApiController
    {
        string clientId = "768ce687-cc5c-4ddb-8eac-8298edb354a3";
        string clientSecret = "ny94x+Uo9nf1XJnoGkyuZSe4MCTJbEIH02NF5PSmauc=";

        public DashboardsController()
        {
            var token = GetToken();
        }

        public string Get()
        {

            return JsonConvert.SerializeObject(output);
        }


        public AuthenticationResult GetToken()
        {
            string output = string.Empty;

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
                {"redirect_uri", "http://localhost:44307/Home/TreatResponse"}
             };

            //Create sign-in query string
            var queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add(@params);

            //Redirect authority
            //Authority Uri is an Azure resource that takes a client id to get an Access token
            string authorityUri = "https://login.windows.net/common/oauth2/authorize/";


            return Redirect(String.Format("{0}?{1}", authorityUri, queryString));
        }

    }
}
