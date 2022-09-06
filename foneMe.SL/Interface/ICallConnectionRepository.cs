using foneMe.SL.Entities;
using foneMe.ViewModels.Account;
using foneMe.ViewModels.Twilio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.SL.Interface
{
    public interface ICallConnectionRepository : IRepository<CallConnection>
    {
        CallConnectResponse SaveCallConnection(CallNotificationVM objCallNotificationVM);
        List<GetCallLogsModel> GetCallConnectionLogs(Guid? UserId);
        List<UserContacts> GetUserMatchContacts(UserContactsVM model);
        List<UserContacts> GetUserContactsPaged(UserContactsVM model);

        bool  AddUserasFriend(AddContactsVM model);



    }
}
