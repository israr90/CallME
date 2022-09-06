using foneMe.SL.Entities;
using foneMe.SL.Interface;
using Microsoft.Owin.Security.DataHandler.Encoder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;

namespace foneMeService.Identity
{
    public class AudienceStore
    {
        private readonly IUnitOfWork _unitOfWork;

        public AudienceStore(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Audience AddAudience(string name)
        {
            var audience = FindAudienceByName(name);
            if (audience != null)
            {
                throw new Exception("Audience with the given name already exists in the Database");
            }
            else
            {
                var clientId = Guid.NewGuid().ToString("N");
                var key = new byte[32];
                RNGCryptoServiceProvider.Create().GetBytes(key);
                var base64Secret = TextEncodings.Base64Url.Encode(key);

                Audience newAudience = new Audience { ClientId = clientId, Base64Secret = base64Secret, Name = name };
                _unitOfWork.AudienceRepository.Add(newAudience);
                _unitOfWork.SaveChanges();
                return newAudience;
            }
        }

        public Audience FindAudience(string clientId)
        {
            return _unitOfWork.AudienceRepository.GetbyClientId(clientId);
        }

        public Audience FindAudienceByName(string name)
        {
            return _unitOfWork.AudienceRepository.GetAudienceByName(name);
        }
    }
}