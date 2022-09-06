using foneMe.SL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace foneMe.SL.Interface
{
    public interface IRoleRepository : IRepository<Role>
    {
        Role FindByName(string roleName);
        Task<Role> FindByNameAsync(string roleName);
        Task<Role> FindByNameAsync(CancellationToken cancellationToken, string roleName);
    }
}
