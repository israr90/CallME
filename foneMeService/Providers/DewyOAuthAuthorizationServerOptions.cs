using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace foneMeService.Providers
{
    public class DewyOAuthAuthorizationServerOptions : IOAuthAuthorizationServerOptions
    {
        private OAuthAuthorizationServerProvider _jwtAuthProvider;
        private ISecureDataFormat<AuthenticationTicket> _jwtDataFormat;

        public DewyOAuthAuthorizationServerOptions(OAuthAuthorizationServerProvider jwtAuthProvider,
           ISecureDataFormat<AuthenticationTicket> jwtDataFormat)
        {
            _jwtAuthProvider = jwtAuthProvider;
            _jwtDataFormat = jwtDataFormat;
        }

        public OAuthAuthorizationServerOptions GetOptions()
        {
            return new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = Boolean.Parse(ConfigurationManager.AppSettings["as:allowInsecure"]),
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(int.Parse(ConfigurationManager.AppSettings["as:tokenExpirationLimitInDays"])),
                Provider = _jwtAuthProvider,
                AccessTokenFormat = _jwtDataFormat
            };
        }
    }
}