using foneMe.SL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.SL.Interface
{
    public interface IAudienceRepository : IRepository<Audience>
    {
        Audience GetAudienceByName(string name);
        Audience GetbyClientId(string cliendId);
    }
}
