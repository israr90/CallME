using System.Web.Mvc;
using System.Web.Routing;

namespace foneMeService
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            
            routes.MapRoute("Default", "{foneme}", (object)new
            {
                controller = "Home",
                action = "AboutMe",
                foneme = "[^\\.]*"
            });
            
            routes.MapRoute(
                name: "GroupChannelPreview",
                url: "g/preview/{name}",
                defaults: new { controller = "GroupChannel", action = "PreviewChannel", name = UrlParameter.Optional });

            routes.MapRoute(
                name: "GroupChannel",
                url: "g/{name}",
                defaults: new { controller = "GroupChannel", action = "Index", name = UrlParameter.Optional });

            routes.MapRoute(
                name: "Home",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Freelancers",
                url: "{controller}/{action}/{searchValue}",
                defaults: new { controller = "Home", action = "Freelancers", searchValue = UrlParameter.Optional }
            );

        }
    }
}
