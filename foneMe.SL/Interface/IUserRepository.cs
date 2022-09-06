using foneMe.SL.Entities;
using foneMe.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.SL.Interface
{
    public interface IUserRepository : IRepository<User>
    {
        IEnumerable<User> Filter(string search);
        IEnumerable<User> FilterUsers(string search, Guid userId);
        User FindByLoginName(string userName);
        User FindByCNIC(string cnic);


        User FindByUserId(Guid userid);

        Task<BoolResultVM> ChangePhoneNumber(ChangePhoneNumberVM model);
        Task<BoolResultVM> SaveUserDOB(UserDOBVM model);
        Task<string> checkNIC(string model);
        Task<BoolResultVM> SaveUserNIC(String nic, String userID);


        User FindByCNCName(string userName);

        User FindBySearchText(string userName);

        Task<BoolResultVM> updateUserDeviceToken(UserTokenModelVM model);

        Task<BoolResultVM> updateUserVoipToken(UserTokenModelVM model);
        Task<string> GetCNICByLoginName(string loginName);
    }
}
