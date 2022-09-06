using foneMeService.Providers;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Jwt;
using Ninject;
using Ninject.Web.WebApi;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;


[assembly: OwinStartup(typeof(foneMeService.App_Start.StartupAuth))]
namespace foneMeService.App_Start
{
    public class StartupAuth
    {
        internal static IDataProtectionProvider DataProtectionProvider { get; private set; }
        private static IKernel kernel;

        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();
            kernel = NinjectWebCommon.GetKernel();
            config.DependencyResolver = new NinjectDependencyResolver(kernel);
            // Web API routes
            config.MapHttpAttributeRoutes();

            ConfigureOAuth(app);

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            app.UseWebApi(config);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            var issuer = ConfigurationManager.AppSettings["as:Issuer"];
            var clientId = ConfigurationManager.AppSettings["as:ClientId"];
            var secret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);

            // OAuth 2.0 Bearer Access Token Generation
            app.UseOAuthAuthorizationServer(kernel.Get<DewyOAuthAuthorizationServerOptions>().GetOptions());

            // move this to be provided by DI , also the clients could be more than 
            // one , currently we are hard coding one client
            // Api controllers with an [Authorize] attribute will be validated with JWT
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    AllowedAudiences = new[] { clientId },
                    IssuerSecurityKeyProviders = new[]
                    {
                        new SymmetricKeyIssuerSecurityKeyProvider(issuer, secret)
                    }
                });
            DataProtectionProvider = app.GetDataProtectionProvider();
        }
    }
}