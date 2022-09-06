using foneMe.SL.Entities;
using foneMe.SL.Interface;
using foneMe.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.DAL.Repositories
{

    internal class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _context;
        internal UserRepository(ApplicationDbContext context)
            : base(context)
        { _context = context; }

        public IEnumerable<User> Filter(string search) => Set.Where(c => c.FirstName.Contains(search) || c.CNIC.Contains(search));
        public IEnumerable<User> FilterUsers(string search, Guid userId) => Set.Where(c => c.UserId.ToString() != userId.ToString() && (c.FirstName.Contains(search) || c.CNIC.Contains(search)));
        public User FindByCNIC(string cnic) => Set.Where(x => x.CNIC == cnic)?.FirstOrDefault();
        public User FindByLoginName(string loginName) => Set.Where(x => x.LoginName == loginName)?.FirstOrDefault();

        public async Task<BoolResultVM> ChangePhoneNumber(ChangePhoneNumberVM model)
        {
            var objUser = _context.Users.Where(x => x.UserId == model.UserId)?.FirstOrDefault();
            var result = new BoolResultVM();
            if (objUser != null)
            {
                if (objUser?.LoginName != model.OldNumber)
                {
                    result = new BoolResultVM
                    {
                        IsSuccessed = false,
                        StatusCode = "100"  // Old Phone Number is not matched
                    };
                }
                else if (objUser.LoginName == model.NewNumber)
                {
                    result = new BoolResultVM
                    {
                        IsSuccessed = false,
                        StatusCode = "101"  // your new number is already your login number.
                    };
                }
                else
                {
                    objUser.LoginName = model.NewNumber;
                    result = new BoolResultVM
                    {
                        IsSuccessed = true,
                        StatusCode = "200"  // Password is changed
                    };
                }
            }
            else
            {
                result = new BoolResultVM
                {
                    IsSuccessed = false,
                    StatusCode = "406" // User does not exist.
                };
            }
            return await Task.FromResult(result);
        }

        public async Task<BoolResultVM> SaveUserDOB(UserDOBVM model)
        {
            var objUser = _context.Users.Where(x => x.UserId == model.UserId)?.FirstOrDefault();
            var result = new BoolResultVM();
            if (objUser != null)
            {
                objUser.DateofBirth = model.DOB;
                result = new BoolResultVM
                {
                    IsSuccessed = true,
                    StatusCode = "200"  // Save DOB
                };
            }
            else
            {
                result = new BoolResultVM
                {
                    IsSuccessed = false,
                    StatusCode = "406" // User does not exist.
                };
            }
            return await Task.FromResult(result);
        }


        public async Task<string> checkNIC(string nic)
        {
            string result = null;
            if (!nic.All(char.IsLetterOrDigit))
            {
                return await Task.FromResult(result);
            }
            else
            {
                string userId = _context.Database.SqlQuery<Guid>(@"SELECT UserId FROM Users WHERE CNIC = '" + nic + "'").FirstOrDefault<Guid>().ToString();
                if (!String.IsNullOrEmpty(userId))
                {
                    result = userId;
                }
                else
                {
                    result = null;
                }
                return await Task.FromResult(result);
            }
        }

        public async Task<BoolResultVM> SaveUserNIC(String nic, String userID)
        {
            var result = new BoolResultVM();
            if (!nic.All(char.IsLetterOrDigit))
            {
                result = new BoolResultVM
                {
                    IsSuccessed = false,
                    StatusCode = "406"
                };
                return await Task.FromResult(result);
            }
            else
            {
                int count = _context.Database.ExecuteSqlCommand(@"UPDATE Users SET CNIC = '" + nic + "' WHERE UserId = '" + userID + "'");
                if (count > 0)
                {
                    result = new BoolResultVM
                    {
                        IsSuccessed = true,
                        StatusCode = "200"
                    };
                }
                else
                {
                    result = new BoolResultVM
                    {
                        IsSuccessed = false,
                        StatusCode = "406"
                    };
                }
                return await Task.FromResult(result);
            }
        }

        public User FindByCNCName(string userName)
        {
            throw new NotImplementedException();
        }

        public User FindBySearchText(string userName)
        {
            throw new NotImplementedException();
        }

        public async Task<BoolResultVM> updateUserDeviceToken(UserTokenModelVM model)
        {
            var objUser = _context.Users.Where(x => x.UserId == model.UserGuid)?.FirstOrDefault();
            var result = new BoolResultVM();
            if (objUser != null)
            {
                objUser.DeviceToken = model.Data;
                _context.SaveChanges();

                result = new BoolResultVM
                {
                    IsSuccessed = true,
                    StatusCode = "200"  // Save DOB
                };
            }
            else
            {
                result = new BoolResultVM
                {
                    IsSuccessed = false,
                    StatusCode = "406" // User does not exist.
                };
            }
            return await Task.FromResult(result);
        }

        public async Task<BoolResultVM> updateUserVoipToken(UserTokenModelVM model)
        {
            var objUser = _context.Users.Where(x => x.UserId == model.UserGuid)?.FirstOrDefault();
            var result = new BoolResultVM();
            if (objUser != null)
            {
                objUser.VoipToken = model.Data;
                _context.SaveChanges();

                result = new BoolResultVM
                {
                    IsSuccessed = true,
                    StatusCode = "200"  // Save DOB
                };
            }
            else
            {
                result = new BoolResultVM
                {
                    IsSuccessed = false,
                    StatusCode = "406" // User does not exist.
                };
            }
            return await Task.FromResult(result);
        }

        public User FindByUserId(Guid userid) => Set.Where(x => x.UserId == userid)?.FirstOrDefault();

        public async Task<string> GetCNICByLoginName(string loginName)
        {
            return await _context.Users.Where(q => q.LoginName == loginName)
                  .Select(x => x.CNIC)
                  .FirstOrDefaultAsync();
        }
    }
}
