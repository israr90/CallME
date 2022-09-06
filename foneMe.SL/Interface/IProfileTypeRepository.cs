using foneMe.SL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.SL.Interface
{
    public interface IProfileTypeRepository : IRepository<ProfileType>
    {
        IEnumerable<ProfileType> Filter(string keyword);
        ProfileType GetFirstByPrefix(string prefix);
        IEnumerable<ProfileType> GetAllByPrefix(string prefix);
    }
}
