using foneMe.ViewModels.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.ViewModels.Account
{
    public class PostVM
    {
        public string PhoneNumber { get; set; }
        public string DeviceToken { get; set; }
        public string VOIPDeviceToken { get; set; }
        public string SMSCode { get; set; }
        public string FatherName { get; set; }
        public bool IsUserTesting { get; set; }
        public Guid UserId { get; set; }
    }

    public class RegisterVM
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public Guid Id { get; set; }
        public string Password { get; set; }
        public string CountryCode { get; set; }
        public string NumberWithOutCode { get; set; }
        public string CNIC { get; set; }
        public string FatherName { get; set; }
        //public string LastName { get; set; }
        //public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        //public string Longitude { get; set; }
        //public string Lattitude { get; set; }
        //public string Gender { get; set; }
        //public bool Update { get; set; }
        public string RoleName { get; set; }
        //public DateTime DateOfBirth { get; internal set; }
    }

    public class AccountViewModel
    {
    }
    public class RegisterStatusVM
    {
        public string StatusCode { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public Guid? UserId { get; set; }
        public UserInfo UserInfo { get; set; }
    }
    public class CnictoProfileVM
    {
        public Guid Me { get; set; }
        public bool IsSuccessed { get; set; }
        public Guid Friend { get; set; }
        public string Url { get; set; }
        public string Cnic { get; set; }
        public Guid? UserId { get; set; }

    }

    public class SearchProfileVM
    {
        public Guid UserId { get; set; }
        public string byName { get; set; }
        public string byCnic { get; set; }
    }


    public class FirebaseVM
    {
        public string DeviceToken { get; set; }
    }

    public class UpdateUserVM
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public string ContactVT { get; set; }
        public string ContactFT { get; set; }
        public string ContactCNIC { get; set; }
        public string Address { get; set; }
        public string FatherName { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string MobileNumberWithoutCode { get; set; }
        public string StatusCode { get; set; }

        public UserAboutMeViewModel AboutMe { get; set; }

    }

    public class LogoutVM
    {
        public Guid UserId { get; set; }

    }

    public class UserInfoModelVM
    {
        public Guid UserId { get; set; }
        public string Data { get; set; }




    }

    public class SendFcmRequestVM
    {
        public string Title { get; set; }
        public string Desc { get; set; }
        public string FcmToken { get; set; }
        public Dictionary<string,string> Data { get; set; }

    }

    public class AddContactsVM
    {
       
        public Guid UserId { get; set; }
        public Guid FriendId { get; set; }
        public string  Name { get; set; }


    }
    public class UserContactsVM
    { 
        public List<UserContacts> Contacts { get; set; }
        public Guid UserId { get; set; }
        public int Page { get; set; }
    }
    public class UserContacts
    {
        public string ContactsCnic { get; set; }
        public string ContactsName { get; set; }
        public string ContactsNumber { get; set; }
        public string ContactsVT { get; set; }

        public string ContactsFT { get; set; }

        public string Image { get; set; }
    }
    public class UserContactsResponse
    {
        public List<UserContacts> Contacts { get; set; }
        public string StatusCode { get; set; }
    }
    public class UserRecoverVM
    {
        public List<UserContacts> UserId { get; set; }
        public string StatusCode { get; set; }
    }
}
