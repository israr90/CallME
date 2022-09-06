using foneMe.SL.Interface;
using foneMe.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace foneMeService.Identity
{
    public class UserStore : IUserLoginStore<IdentityUser, Guid>,
       IUserClaimStore<IdentityUser, Guid>,
       IUserRoleStore<IdentityUser, Guid>, IUserPasswordStore<IdentityUser, Guid>,
       IUserSecurityStampStore<IdentityUser, Guid>, IUserStore<IdentityUser, Guid>,
       IUserEmailStore<IdentityUser, Guid>,
       IUserPhoneNumberStore<IdentityUser, Guid>,
       IDisposable
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserStore(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        #region IUserStore<IdentityUser, Guid> Members
        public Task CreateAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var u = getUser(user);

            _unitOfWork.UserRepository.Add(u);
            return _unitOfWork.SaveChangesAsync();
        }

        public Task DeleteAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var u = getUser(user);

            _unitOfWork.UserRepository.Remove(u);
            return _unitOfWork.SaveChangesAsync();
        }

        public Task<IdentityUser> FindByIdAsync(Guid userId)
        {
            var user = _unitOfWork.UserRepository.FindById(userId);
            return Task.FromResult<IdentityUser>(getIdentityUser(user));
        }

        public Task<IdentityUser> FindByNameAsync(string userName)
        {
            var user = _unitOfWork.UserRepository.FindByLoginName(userName);
            return Task.FromResult<IdentityUser>(getIdentityUser(user));
        }

        public Task UpdateAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentException("user");

            var u = _unitOfWork.UserRepository.FindById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            populateUser(u, user);

            _unitOfWork.UserRepository.Update(u);
            return _unitOfWork.SaveChangesAsync();
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            // Dispose does nothing since we want Ninject to manage the lifecycle of our Unit of Work
        }
        #endregion

        #region IUserClaimStore<IdentityUser, Guid> Members
        public Task AddClaimAsync(IdentityUser user, Claim claim)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (claim == null)
                throw new ArgumentNullException("claim");

            var u = _unitOfWork.UserRepository.FindById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            var c = new foneMe.SL.Entities.Claim
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                User = u
            };
            u.Claims.Add(c);

            _unitOfWork.UserRepository.Update(u);
            return _unitOfWork.SaveChangesAsync();
        }

        public Task<IList<Claim>> GetClaimsAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var u = _unitOfWork.UserRepository.FindById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            return Task.FromResult<IList<Claim>>(u.Claims.Select(x => new Claim(x.ClaimType, x.ClaimValue)).ToList());
        }

        public Task RemoveClaimAsync(IdentityUser user, Claim claim)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (claim == null)
                throw new ArgumentNullException("claim");

            var u = _unitOfWork.UserRepository.FindById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            var c = u.Claims.FirstOrDefault(x => x.ClaimType == claim.Type && x.ClaimValue == claim.Value);
            u.Claims.Remove(c);

            _unitOfWork.UserRepository.Update(u);
            return _unitOfWork.SaveChangesAsync();
        }
        #endregion

        #region IUserLoginStore<IdentityUser, Guid> Members
        public Task AddLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (login == null)
                throw new ArgumentNullException("login");

            var u = _unitOfWork.UserRepository.FindById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            var l = new foneMe.SL.Entities.ExternalLogin
            {
                LoginProvider = login.LoginProvider,
                ProviderKey = login.ProviderKey,
                User = u
            };
            u.ExternalLogins.Add(l);

            _unitOfWork.UserRepository.Update(u);
            return _unitOfWork.SaveChangesAsync();
        }

        public Task<IdentityUser> FindAsync(UserLoginInfo login)
        {
            if (login == null)
                throw new ArgumentNullException("login");

            var identityUser = default(IdentityUser);

            var l = _unitOfWork.ExternalLoginRepository.GetByProviderAndKey(login.LoginProvider, login.ProviderKey);
            if (l != null)
                identityUser = getIdentityUser(_unitOfWork.UserRepository.FindById(l.UserId));

            return Task.FromResult<IdentityUser>(identityUser);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var u = _unitOfWork.UserRepository.FindById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            return Task.FromResult<IList<UserLoginInfo>>(u.ExternalLogins.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey)).ToList());
        }

        public Task RemoveLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (login == null)
                throw new ArgumentNullException("login");

            var u = _unitOfWork.UserRepository.FindById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            var l = u.ExternalLogins.FirstOrDefault(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
            u.ExternalLogins.Remove(l);

            _unitOfWork.UserRepository.Update(u);
            return _unitOfWork.SaveChangesAsync();
        }
        #endregion

        #region IUserRoleStore<IdentityUser, Guid> Members
        public Task AddToRoleAsync(IdentityUser user, string roleName)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Argument cannot be null, empty, or whitespace: roleName.");

            var u = _unitOfWork.UserRepository.FindById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");
            var r = _unitOfWork.RoleRepository.FindByName(roleName);
            if (r == null)
                throw new ArgumentException("roleName does not correspond to a Role entity.", "roleName");

            u.Roles.Add(r);
            _unitOfWork.UserRepository.Update(u);

            return _unitOfWork.SaveChangesAsync();
        }

        public Task<IList<string>> GetRolesAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");

            var u = _unitOfWork.UserRepository.FindById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            return Task.FromResult<IList<string>>(u.Roles.Select(x => x.Name).ToList());
        }

        public Task<bool> IsInRoleAsync(IdentityUser user, string roleName)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Argument cannot be null, empty, or whitespace: role.");

            var u = _unitOfWork.UserRepository.FindById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            return Task.FromResult<bool>(u.Roles.Any(x => x.Name == roleName));
        }

        public Task RemoveFromRoleAsync(IdentityUser user, string roleName)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            if (string.IsNullOrWhiteSpace(roleName))
                throw new ArgumentException("Argument cannot be null, empty, or whitespace: role.");

            var u = _unitOfWork.UserRepository.FindById(user.Id);
            if (u == null)
                throw new ArgumentException("IdentityUser does not correspond to a User entity.", "user");

            var r = u.Roles.FirstOrDefault(x => x.Name == roleName);
            u.Roles.Remove(r);

            _unitOfWork.UserRepository.Update(u);
            return _unitOfWork.SaveChangesAsync();
        }
        #endregion

        #region IUserPasswordStore<IdentityUser, Guid> Members
        public Task<string> GetPasswordHashAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult<string>(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult<bool>(!string.IsNullOrWhiteSpace(user.PasswordHash));
        }

        public Task SetPasswordHashAsync(IdentityUser user, string passwordHash)
        {
            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }
        #endregion

        #region IUserSecurityStampStore<IdentityUser, GUID> Members
        public Task<string> GetSecurityStampAsync(IdentityUser user)
        {
            if (user == null)
                throw new ArgumentNullException("user");
            return Task.FromResult<string>(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(IdentityUser user, string stamp)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }
        #endregion

        #region Private Methods
        private foneMe.SL.Entities.User getUser(IdentityUser identityUser)
        {
            if (identityUser == null)
                return null;

            var user = new foneMe.SL.Entities.User();
            populateUser(user, identityUser);

            return user;
        }

        private void populateUser(foneMe.SL.Entities.User user, IdentityUser identityUser)
        {
            // Create User Id according to key table in the database
            // update count on each user creation.
            identityUser.UserName = "+" + identityUser.UserName;
            user.UserId = identityUser.Id;
            user.LoginName = identityUser.UserName;
            user.PasswordHash = identityUser.PasswordHash;
            user.SecurityStamp = identityUser.SecurityStamp;
            user.CNIC = identityUser.CNIC;

            //login.User.Ven
            user.FirstName = identityUser.FirstName;
            user.FullName = identityUser.FirstName;
            user.Gender = identityUser.Gender;
            user.EmailConfirmed = false;
            user.IsDeleted = false;
            user.LockOutEnabled = false;
            user.ModifiedAt = DateTime.Now;
            user.AreaId = identityUser.AreaId;
            user.IsUserVerified = false;
            user.ModifiedBy = identityUser.UserName;

            // check if RoleId is null, we leave it null in case of customer
            // in case of labContact the admin selects a role
            if (string.IsNullOrEmpty(identityUser.RoleName))
            {
                //var contactNo = 1;
                if (!(string.IsNullOrEmpty(identityUser.PhoneNo)))
                {
                    var contact = new foneMe.SL.Entities.Contact
                    {
                        //ContactId = _unitOfWork.ContactRepository.GenerateId() + (contactNo++),
                        UserId = user.UserId,
                       
                        ContactTypeProfileId = _unitOfWork.ProfileRepository.GetByShortName("MBN")?.ProfileId,
                        ContactCategoryProfileId = _unitOfWork.ProfileRepository.GetByShortName("PRN")?.ProfileId,
                        Description = identityUser.PhoneNo,
                        CountryCode = identityUser.CountryCode,
                        NumberWithOutCode = identityUser.NumberWithOutCode,
                        IsActive = true,
                        IsPrimary = true,
                    };
                    _unitOfWork.ContactRepository.Add(contact);
                }

                if (!(string.IsNullOrEmpty(identityUser.Email)))
                {
                    var contact = new foneMe.SL.Entities.Contact
                    {
                        //ContactId = _unitOfWork.ContactRepository.GenerateId() + (contactNo++),
                        UserId = user.UserId,
                        ContactTypeProfileId = _unitOfWork.ProfileRepository.GetByShortName("EML")?.ProfileId,
                        ContactCategoryProfileId = _unitOfWork.ProfileRepository.GetByShortName("PRN")?.ProfileId,
                        Description = identityUser.Email,
                        IsActive = true,
                        IsPrimary = true,
                    };
                    _unitOfWork.ContactRepository.Add(contact);
                }

                if (!(string.IsNullOrEmpty(identityUser.Address)))
                {
                    var contact = new foneMe.SL.Entities.Contact
                    {
                        //ContactId = _unitOfWork.ContactRepository.GenerateId() + (contactNo++),
                        UserId = user.UserId,
                        ContactTypeProfileId = _unitOfWork.ProfileRepository.GetByShortName("ADD")?.ProfileId,
                        ContactCategoryProfileId = _unitOfWork.ProfileRepository.GetByShortName("PRN")?.ProfileId,
                        //AreaId = user.AreaId,
                        Description = identityUser.Address,
                        IsActive = true,
                        IsPrimary = true,
                    };
                    _unitOfWork.ContactRepository.Add(contact);
                }
                // only execute this code if this is not update case
                if (!(identityUser.Update))
                {
                    var role = _unitOfWork.RoleRepository.FindByName("Customer");
                    user.Roles.Add(role);
                }
            }
        }

        private IdentityUser getIdentityUser(foneMe.SL.Entities.User user)
        {
            if (user == null)
                return null;

            var identityUser = new IdentityUser();
            populateIdentityUser(identityUser, user);

            return identityUser;
        }

        private void populateIdentityUser(IdentityUser identityUser, foneMe.SL.Entities.User user)
        {
            if (user == null)
            {
                user = _unitOfWork.UserRepository.FindById(user.UserId);
            }

            if (identityUser.Contacts == null || identityUser.Contacts.Count < 1)
            {
                if (user.Contacts != null && user.Contacts.Count > 0)
                {
                    identityUser.Contacts = new List<ContactViewModel>();
                    foreach (foneMe.SL.Entities.Contact contact in user.Contacts)
                    {
                        ContactViewModel con = new ContactViewModel
                        {
                            ContactCategoryProfileId = contact.ContactCategoryProfileId ?? 0,
                            ContactTypeProfileId = contact.ContactTypeProfileId ?? 0,
                            Description = contact.Description ?? ""
                        };
                        identityUser.Contacts.Add(con);
                    }
                }
            }
            var emailTypeId = _unitOfWork?.ProfileRepository?.GetByShortName("EML")?.ProfileId;
            identityUser.Id = user.UserId;
            identityUser.Update = true;
            identityUser.UserName = user.LoginName;
            identityUser.PasswordHash = user.PasswordHash;
            identityUser.SecurityStamp = user.SecurityStamp;
            identityUser.FirstName = user.FirstName;
            identityUser.LastName = user.LastName;
            identityUser.Gender = user.Gender; ;
            identityUser.RoleName = user.Roles?.FirstOrDefault()?.Name ?? "Customer";
            identityUser.DateOfBirth = user.DateofBirth ?? new DateTime(2000, 1, 1);
        }

        public Task SetEmailAsync(IdentityUser login, string email)
        {
            var dbUser = _unitOfWork.UserRepository.FindByLoginName(email);

            if (dbUser == null)
            {
                throw new ArgumentException("Email is invalid");
            }

            dbUser.LoginName = email;
            _unitOfWork.UserRepository.Update(dbUser);
            return _unitOfWork.SaveChangesAsync();
        }

        public Task<string> GetEmailAsync(IdentityUser user)
        {
            var dbUser = _unitOfWork.UserRepository.FindById(user.Id);

            if (dbUser == null)
            {
                throw new ArgumentException("Email is invalid");
            }
            var email = dbUser.LoginName;
            return Task.FromResult(email);
        }

        public Task<bool> GetEmailConfirmedAsync(IdentityUser user)
        {
            var dbUser = _unitOfWork.UserRepository.FindById(user.Id);

            if (dbUser == null)
            {
                throw new ArgumentException("Email is invalid");
            }
            var email = dbUser.EmailConfirmed ?? false;
            return Task.FromResult(email);
        }

        public Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed)
        {
            var dbUser = _unitOfWork.UserRepository.FindById(user.Id);

            if (dbUser == null)
            {
                throw new ArgumentException("Email is invalid");
            }
            dbUser.EmailConfirmed = true;
            _unitOfWork.UserRepository.Update(dbUser);
            populateIdentityUser(user, dbUser);
            return _unitOfWork.SaveChangesAsync();
        }

        public Task<IdentityUser> FindByEmailAsync(string email)
        {
            var dbUser = _unitOfWork.UserRepository.FindByLoginName(email);

            if (dbUser == null)
            {
                throw new ArgumentException("Email is invalid");
            }
            var identityUser = getIdentityUser(dbUser);
            return Task.FromResult(identityUser);
        }

        public Task SetPhoneNumberAsync(IdentityUser login, string phoneNumber)
        {
            var dbUser = _unitOfWork.UserRepository.FindById(login.Id);
            var ContactTypeProfileId = _unitOfWork.ProfileRepository.GetByShortName("ADD")?.ProfileId;
            var ContactCategoryProfileId = _unitOfWork.ProfileRepository.GetByShortName("PRN")?.ProfileId;
            var contact = new foneMe.SL.Entities.Contact();
            contact.ContactCategoryProfileId = ContactCategoryProfileId;
            contact.ContactTypeProfileId = ContactTypeProfileId;
            contact.Description = phoneNumber;
            dbUser.Contacts.Add(contact);
            _unitOfWork.UserRepository.Update(dbUser);
            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(IdentityUser user)
        {
            var dbUser = _unitOfWork.UserRepository.FindById(user.Id);

            if (dbUser == null)
            {
                throw new ArgumentException("Email is invalid");
            }
            var match = Regex.Match(dbUser.LoginName, @"^((\+92)|(0092))-{0,1}\d{3}-{0,1}\d{7}$|^\d{11}$|^\d{4}-\d{7}$");
            if (match.Success)
            {
                return Task.FromResult(dbUser.LoginName);
            }
            else
            {
                var ContactTypeProfileId = _unitOfWork.ProfileRepository.GetByShortName("ADD")?.ProfileId;
                var ContactCategoryProfileId = _unitOfWork.ProfileRepository.GetByShortName("PRN")?.ProfileId;
                var phoneNumber = dbUser.Contacts.Where(x => x.ContactTypeProfileId == ContactTypeProfileId && x.ContactCategoryProfileId == ContactCategoryProfileId)?.FirstOrDefault()?.Description ?? "";
                return Task.FromResult(phoneNumber);
            }

        }

        public Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user)
        {
            var dbUser = _unitOfWork.UserRepository.FindById(user.Id);

            if (dbUser == null)
            {
                throw new ArgumentException("Email is invalid");
            }
            var email = dbUser.EmailConfirmed ?? false;
            return Task.FromResult(email);
        }

        public Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed)
        {
            var dbUser = _unitOfWork.UserRepository.FindById(user.Id);

            if (dbUser == null)
            {
                throw new ArgumentException("Email is invalid");
            }
            dbUser.EmailConfirmed = true;
            _unitOfWork.UserRepository.Update(dbUser);
            populateIdentityUser(user, dbUser);
            return _unitOfWork.SaveChangesAsync();
        }


        #endregion
    }
}