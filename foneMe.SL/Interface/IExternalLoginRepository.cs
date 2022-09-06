using foneMe.SL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace foneMe.SL.Interface
{
    public interface IExternalLoginRepository : IRepository<ExternalLogin>
    {
        ExternalLogin GetByProviderAndKey(string loginProvider, string providerKey);
        Task<ExternalLogin> GetByProviderAndKeyAsync(string loginProvider, string providerKey);
        Task<ExternalLogin> GetByProviderAndKeyAsync(CancellationToken cancellationToken, string loginProvider, string providerKey);
    }
}
