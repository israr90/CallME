using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace foneMeService
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            //AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            //FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Enable Firebase Admin
            if (FirebaseApp.DefaultInstance == null)
            {
                string FirebaseAdminSDKFile = WebConfigurationManager.AppSettings["FirebaseAdminSDKFile"];
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromFile(HostingEnvironment.MapPath(@FirebaseAdminSDKFile))
                });
            }
        }
    }
}
