using foneMe.SL.Entities;
using foneMe.ViewModels.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.SL.Interface
{
    public interface IProfileRepository : IRepository<Profile>
    {
        IEnumerable<Profile> Filter(string keyword);
        int GenerateId();
        Profile GetByShortName(string prefix);
        IEnumerable<Profile> GetByProfileTypeId(int profileTypeId);
        Task<BoolResultVM> GetPrivatePolicy();
        Task<BoolResultVM> TermsAndConditions();

    }
}
