using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.ViewModels.Account
{

    public class MessageCodeVM
    {
        public string SMSCode { get; set; }
        public Guid UserId { get; set; }
        public string StatusCode { get; set; }
        public bool IsUserRegistered { get; set; }
        public bool IsUserVerified { get; set; }
    }
    public class VerifyMessageCodeVM
    {
        public string StatusCode { get; set; }
        public bool IsUSerRegistered { get; set; }
        public bool IsUserVerified { get; set; }
        public UserInfo UserInfo { get; set; }
    }
    public class UserInfo
    {
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string PhoneNumberWithoutCode { get; set; }
        public string LoginName { get; set; }
        public Guid UserId { get; set; }
        public string ImageUrl { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
    }

    public class UserLoginVM
    {
        public bool IsUserVerified { get; set; }
        public Guid UserId { get; set; }
        public string StatusCode { get; set; }
        public bool IsUserRegistered { get; set; }

    }
}
