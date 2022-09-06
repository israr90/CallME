using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace foneMeService.Identity
{
    public class ApplicationUserManager : UserManager<IdentityUser, Guid>
    {
        public ApplicationUserManager(IUserSecurityStampStore<IdentityUser, Guid> store)
            : base(store)
        {
        }

        // *** some other code

        public override async Task<string> GenerateChangePhoneNumberTokenAsync(Guid userId, string phoneNumber)
        {
            var user = await FindByIdAsync(userId);
            var code = "";
            //var code = CustomRfc6238AuthenticationService.GenerateCode(user.SecurityStamp, phoneNumber, "optional modifier", TimeSpan.FromDays(14));
            return code;
        }
    }
}