using foneMe.SL.Interface;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace foneMeService.Identity
{
    public class ClaimStore : IUserClaimStore<IdentityUser, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClaimStore(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task AddClaimAsync(IdentityUser user, Claim claim)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(IdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(IdentityUser user)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<IdentityUser> FindByIdAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityUser> FindByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Claim>> GetClaimsAsync(IdentityUser user)
        {
            throw new NotImplementedException();
        }

        public Task RemoveClaimAsync(IdentityUser user, Claim claim)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(IdentityUser user)
        {
            throw new NotImplementedException();
        }
    }
}