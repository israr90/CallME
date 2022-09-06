using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace foneMe.SL.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        #region Properties
        IExternalLoginRepository ExternalLoginRepository { get; }
        IRoleRepository RoleRepository { get; }
        IUserRepository UserRepository { get; }
        IAudienceRepository AudienceRepository { get; }
        IProfileRepository ProfileRepository { get; }
        IProfileTypeRepository ProfileTypeRepository { get; }
        ICommonRepository CommonRepository { get; }
        IContactRepository ContactRepository { get; }
        ICallConnectionRepository CallConnectionRepository { get; }
        IGroupChannelRepository GroupChannelRepository { get; }
        IUserAboutmeRepository UserAboutmeRepository { get; }


        #endregion

        #region Methods
        int SaveChanges();
        Task<int> SaveChangesAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        #endregion
    }
}
