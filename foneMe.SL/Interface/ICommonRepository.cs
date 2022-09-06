using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.SL.Interface
{
    public interface ICommonRepository
    {
        //Document GetDocumentById(long documentId);
        //List<MembershipProfileType> GetMemberShipPlanTypes();
        //List<MembershipDayScheduleDelivery> GetMemberShipDayScheduleDelivery();
        string GetLocalContactNameCaller(Guid dialerUserId, Guid haveLocalContactsUserId);
    }
}
