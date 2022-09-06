using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.ViewModels.Account
{
    public class ChangePhoneNumberVM
    {
        public string NewNumber { get; set; }
        public string OldNumber { get; set; }
        public Guid UserId { get; set; }
    }
}
