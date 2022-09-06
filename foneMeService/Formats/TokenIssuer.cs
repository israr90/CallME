using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace foneMeService.Formats
{
    public class TokenIssuer : ITokenIssuer
    {
        public string TokenIssuerServer()
        {
            return ConfigurationManager.AppSettings["as:Issuer"];
        }
    }
}