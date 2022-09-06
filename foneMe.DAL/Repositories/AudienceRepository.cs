using foneMe.SL.Entities;
using foneMe.SL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.DAL.Repositories
{
    public class AudienceRepository : Repository<Audience>, IAudienceRepository
    {
        internal AudienceRepository(ApplicationDbContext dbContext) : base(dbContext)
        { }

        public Audience GetAudienceByName(string name)
        {
            return Set.Where(x => x.Name == name)?.FirstOrDefault();
        }

        public Audience GetbyClientId(string cliendId)
        {
            return Set.Where(x => x.ClientId == cliendId)?.FirstOrDefault();
        }
    }
}
