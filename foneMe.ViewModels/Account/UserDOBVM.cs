using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.ViewModels.Account
{
    public class UserDOBVM
    {
        public Guid UserId { get; set; }
        public DateTime DOB { get; set; }
    }
    public class UserTokenModelVM
    {
        public Guid UserGuid { get; set; }
        public string Data { get; set; }
    }
    public class GeoLocation
    {
        public double Lattitude { get; set; }
        public double Longitude { get; set; }
    }

    public class UserProfileVM
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Image { get; set; }
        public string FatherName { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public byte[] UserImage { get; set; }
        public string FileName { get; set; }

    }
   
}
