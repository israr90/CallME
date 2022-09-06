using foneMe.SL.Entities;
using foneMe.ViewModels.Account;
using foneMe.ViewModels.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.SL.Interface
{
  
    public interface IGroupChannelRepository : IRepository<CallConnection>
    {
        void AddGroup(GroupDetailsViewModel model);
        void UpdateGroupInfo(GroupDetailsViewModel model);
        List<GroupDetailsViewModel> GetSingleGroup(GroupDetailsViewModel model);
        List<GroupDetailsViewModel> GetGroupByName(GroupDetailsViewModel model);
        GroupDetailsViewModel GetGroupByDeepLink(GroupDetailsViewModel model);
        GroupDetailsViewModel GetGroupByURL(GroupDetailsViewModel model);
        List<GroupDetailsViewModel> GetAllGroups(GroupDetailsViewModel model);
    }
}
