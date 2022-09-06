using foneMe.SL.Entities;
using foneMe.SL.Interface;
using foneMeService.Identity;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Web;

namespace foneMeService.Formats
{
    public class JwtDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private const string AudiencePropertyKey = "audience";
        private readonly string _issuer = string.Empty;
        private readonly AudienceStore _audienceStore;
        private readonly IUnitOfWork _unitOfWork;

        public JwtDataFormat(ITokenIssuer issuer, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _audienceStore = new AudienceStore(_unitOfWork);
            _issuer = issuer.TokenIssuerServer();
        }

        public string Protect(AuthenticationTicket data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            string audienceId = data.Properties.Dictionary.ContainsKey(AudiencePropertyKey) ? data.Properties.Dictionary[AudiencePropertyKey] : null;

            if (string.IsNullOrWhiteSpace(audienceId)) throw new InvalidOperationException("AuthenticationTicket.Properties does not include audience");

            Audience audience = _audienceStore.FindAudience(audienceId);

            string symmetricKeyAsBase64 = audience.Base64Secret;

            var keyByteArray = TextEncodings.Base64Url.Decode(symmetricKeyAsBase64);

            var signingKey = new HmacSigningCredentials(keyByteArray);

            var issued = data.Properties.IssuedUtc;
            var expires = data.Properties.ExpiresUtc;

            var token = new JwtSecurityToken(_issuer,
                                                audienceId,
                                                data.Identity.Claims,
                                                issued.Value.UtcDateTime,
                                                expires.Value.UtcDateTime,
                                                signingKey);

            var handler = new JwtSecurityTokenHandler();

            var jwt = handler.WriteToken(token);

            return jwt;
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }
}