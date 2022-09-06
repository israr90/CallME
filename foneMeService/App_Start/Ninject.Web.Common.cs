[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(foneMeService.App_Start.NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(foneMeService.App_Start.NinjectWebCommon), "Stop")]

namespace foneMeService.App_Start
{
    using foneMe.DAL;
    using foneMe.SL.Interface;
    using foneMeService.Formats;
    using foneMeService.Identity;
    using foneMeService.Providers;
    using Microsoft.AspNet.Identity;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.DataHandler.Encoder;
    using Microsoft.Owin.Security.DataHandler.Serializer;
    using Microsoft.Owin.Security.DataProtection;
    using Microsoft.Owin.Security.OAuth;
    using Microsoft.Web.Infrastructure.DynamicModuleHelper;
    using Ninject;
    using Ninject.Web.Common;
    using Ninject.Web.Common.WebHost;
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Web;
  

    public static class NinjectWebCommon
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();
        private static IKernel kernel;

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            kernel?.Dispose();
            bootstrapper.ShutDown();
        }

        public static IKernel GetKernel()
        {
            return kernel;
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();
                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            var connectionStringName = ConfigurationManager.AppSettings["cs:connectionStringName"];
            kernel.Bind<IUnitOfWork>().ToConstructor(unit => new UnitOfWork(connectionStringName));
            kernel.Bind<ITextEncoder>().To<Base64UrlTextEncoder>().InTransientScope();
            kernel.Bind<IDataSerializer<AuthenticationTicket>>().To<TicketSerializer>().InTransientScope();
            kernel.Bind<IDataProtector>().ToMethod(x => new DpapiDataProtectionProvider()
                                         .Create("Dewi_Web_Services"));
            kernel.Bind<IUserStore<IdentityUser, Guid>>().To<UserStore>().InTransientScope();
            kernel.Bind<IRoleStore<IdentityRole, Guid>>().To<RoleStore>().InTransientScope();
            kernel.Bind<ITokenIssuer>().To<TokenIssuer>();
            kernel.Bind<OAuthAuthorizationServerProvider>().To<JwtOAuthProvider>();
            kernel.Bind<ISecureDataFormat<AuthenticationTicket>>().To<JwtDataFormat>();
            kernel.Bind<IOAuthAuthorizationServerOptions>().To<DewyOAuthAuthorizationServerOptions>();
        }
    }
}