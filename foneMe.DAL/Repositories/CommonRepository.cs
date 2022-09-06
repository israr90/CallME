using foneMe.SL.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.DAL.Repositories
{
    public class CommonRepository : ICommonRepository
    {
        private readonly ApplicationDbContext _context;

        internal CommonRepository(ApplicationDbContext context)
        { _context = context; }

        //public Document GetDocumentById(long documentId)
        //{
        //    return _context.Documents.FirstOrDefault(x => x.DocumentId == documentId);
        //}
        //public List<MembershipProfileType> GetMemberShipPlanTypes()
        //{
        //    var profileTypeId = _context.ProfileTypes.Where(x => x.ShortName == "MTYP")?.FirstOrDefault()?.ProfileTypeId;
        //    return _context.Profiles.Where(x => x.ProfileTypeId == profileTypeId)?.Select(x => new MembershipProfileType
        //    {
        //        ProfileId = x.ProfileId,
        //        Name = x.Name
        //    })?.ToList();
        //}
        //public List<MembershipDayScheduleDelivery> GetMemberShipDayScheduleDelivery()
        //{
        //    var profileTypeId = _context.ProfileTypes.Where(x => x.ShortName == "MDSD")?.FirstOrDefault()?.ProfileTypeId;
        //    return _context.Profiles.Where(x => x.ProfileTypeId == profileTypeId)?.Select(x => new MembershipDayScheduleDelivery
        //    {
        //        ProfileId = x.ProfileId,
        //        Name = x.Name
        //    })?.ToList();
        //}

        public string GetLocalContactNameCaller(Guid dialerUserId, Guid haveLocalContactsUserId)
        {
            try
            {
               return _context.LocalContacts.Where(x => x.LocalContactUserId == dialerUserId && x.HaveContactsUserId== haveLocalContactsUserId)?.FirstOrDefault()?.Name;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
    }
}
