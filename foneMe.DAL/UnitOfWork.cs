using foneMe.DAL.Repositories;
using foneMe.SL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace foneMe.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        #region fields
        private readonly ApplicationDbContext _context;
        private IExternalLoginRepository _externalLoginRepository;
        private IRoleRepository _roleRepository;
        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
        private IProfileTypeRepository _profileTypeRepository;
        private IAudienceRepository _audienceRepository;
        private ICommonRepository _commonRepository;
        private IContactRepository _contactRepository;
        private ICallConnectionRepository _callConnectionRepository;
        private IGroupChannelRepository _groupChannelRepository;
        private IUserAboutmeRepository _userAboutmeRepository;
        #endregion

        public UnitOfWork(string nameOrConnectionString)
        {
            _context = new ApplicationDbContext(nameOrConnectionString);
        }

        #region public members
        public ICommonRepository CommonRepository
        {
            get { return _commonRepository ?? (_commonRepository = new CommonRepository(_context)); }
        }
        public ICallConnectionRepository CallConnectionRepository => _callConnectionRepository ??
        (_callConnectionRepository = new CallConnectionRepository(_context));
        public IExternalLoginRepository ExternalLoginRepository => _externalLoginRepository ??
            (_externalLoginRepository = new ExternalLoginRepository(_context));
        public IRoleRepository RoleRepository =>
       _roleRepository ?? (_roleRepository = new RoleRepository(_context));
        public IContactRepository ContactRepository =>
            _contactRepository ?? (_contactRepository = new ContactRepository(_context));

        public IUserRepository UserRepository =>
            _userRepository ?? (_userRepository = new UserRepository(_context));

        public IAudienceRepository AudienceRepository =>
            _audienceRepository ?? (_audienceRepository = new AudienceRepository(_context));

        public IProfileRepository ProfileRepository =>
            _profileRepository ?? (_profileRepository = new ProfileRepository(_context));

        public IProfileTypeRepository ProfileTypeRepository =>
            _profileTypeRepository ?? (_profileTypeRepository = new ProfileTypeRepository(_context));

        public IGroupChannelRepository GroupChannelRepository => _groupChannelRepository ?? (_groupChannelRepository = new GroupChannelRepository(_context));
        public IUserAboutmeRepository UserAboutmeRepository => _userAboutmeRepository ?? (_userAboutmeRepository = new UserAboutmeRepository(_context));
        public int SaveChanges() => _context.SaveChanges();

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);

        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            _context?.Dispose();
        }
        #endregion
    }
}
