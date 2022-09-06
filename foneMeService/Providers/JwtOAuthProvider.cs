using foneMe.DAL;
using foneMeService.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace foneMeService.Providers
{
    public class JwtOAuthProvider : OAuthAuthorizationServerProvider
    {
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId = string.Empty;
            string clientSecret = string.Empty;
            string symmetricKeyAsBase64 = string.Empty;
            var connectionStringName = ConfigurationManager.AppSettings["cs:connectionStringName"];

            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                context.TryGetFormCredentials(out clientId, out clientSecret);
            }

            if (context.ClientId == null)
            {
                context.SetError("invalid_clientId", "client_Id is not set");
                return Task.FromResult<object>(null);
            }

            using (var _unitOfWork = new UnitOfWork(connectionStringName))
            {
                var _audienceStore = new AudienceStore(_unitOfWork);
                var audience = _audienceStore.FindAudience(context.ClientId);
                if (audience == null)
                {
                    context.SetError("invalid_clientId", string.Format("Invalid client_id '{0}'", context.ClientId));
                    return Task.FromResult<object>(null);
                }
                context.Validated();
                return Task.FromResult<object>(null);
            }
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var connectionStringName = ConfigurationManager.AppSettings["cs:connectionStringName"];
            // we need to make unit of work transient here
            // for some reason singleton unitofwork cache user
            // this creates problems for clients. As change of password
            // is not ownered. The below peace of code keeps authorizing 
            // user using the previous password
            using (var _unitOfWork = new UnitOfWork(connectionStringName))
            {
                using (UserManager<IdentityUser, Guid> userManager = new UserManager<IdentityUser, Guid>(new UserStore(_unitOfWork)))
                {
                    context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

                    var user = await userManager.FindAsync(context.UserName, context.Password);

                    if (user == null)
                    {
                        context.SetError("invalid_grant", "The user name or password is incorrect.");
                        return;
                    }

                    ClaimsIdentity oAuthIdentity = await userManager.CreateIdentityAsync(user,
                        context.Options.AuthenticationType);

                    oAuthIdentity.AddClaim(new Claim("uid", user.Id.ToString()));
                    var props = new AuthenticationProperties(new Dictionary<string, string>
                            {
                                {
                                    "audience", (context.ClientId == null) ? string.Empty : context.ClientId
                                },
                                {
                                    "userId", user.Id.ToString()
                                }
                            });
                    var ticket = new AuthenticationTicket(oAuthIdentity, props);
                    context.Validated(ticket);
                }
            }
        }
    }
}