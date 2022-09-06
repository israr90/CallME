using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMeService.Providers
{
    public interface IOAuthAuthorizationServerOptions
    {
        OAuthAuthorizationServerOptions GetOptions();
    }
}
