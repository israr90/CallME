using foneMe.SL.Entities;
using foneMe.SL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.DAL.Repositories
{
    public class ProfileTypeRepository : Repository<ProfileType>, IProfileTypeRepository
    {
        internal ProfileTypeRepository(ApplicationDbContext context) : base(context)
        { }

        public IEnumerable<ProfileType> Filter(string keyword)
        {
            int areaId = 0;
            if (int.TryParse(keyword, out areaId))
            {
                var area = this.FindById(areaId); ;
                if (area != null)
                {
                    var singleItemList = new List<ProfileType> { area };
                    return singleItemList;
                }
            }

            return Set.Where(c => c.Name.Contains(keyword))?.ToList();
        }

        public ProfileType GetFirstByPrefix(string prefix)
        {
            return Set.Where(x => x.ShortName == prefix)?.FirstOrDefault();
        }

        public IEnumerable<ProfileType> GetAllByPrefix(string prefix)
        {
            return Set.Where(x => x.ShortName == prefix)?.ToList();
        }
    }
}
