using foneMe.SL.Entities;
using foneMe.SL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.DAL.Repositories
{
    public class ContactRepository : Repository<Contact>, IContactRepository
    {
        internal ContactRepository(ApplicationDbContext context) : base(context)
        { }
    }
}
