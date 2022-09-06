using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.ViewModels.Model
{
    public class UserAboutMeViewModel
    {
        public int Id { get; set; }
        public string UserID { get; set; }
        public string ImageURL { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FoneMe { get; set; }
        public string PhoneModel { get; set; }
        public string PhoneBrand { get; set; }
        public string Profession { get; set; }
        public string Address { get; set; }
        public string AboutMe { get; set; }
        public string Distance { get; set; }
        public string ContactsName { get; set; }
        public string ContactsCnic { get; set; }
        public string ContactsNumber { get; set; }
        public string ContactsVT { get; set; }
        public string ContactsFT { get; set; }

        public List<UserSocialLink> UserAboutMeLink { get; set; }

    }

    public class UserAboutMeResponse
    {
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public UserAboutMeViewModel UserAboutMeData { get; set; }

    }

    public class SearchByProfessionResponse
    {
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public List<UserAboutMeViewModel> UserAboutMeData { get; set; }

    }
    public class SearchUserNearMeViewModel
    {
        public String UserID { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Radius { get; set; }
        public String Unit { get; set; }
        

    }
    public class UserSocialLink
    {
        public int Id { get; set; }
        public string UserID { get; set; }
        public string Name { get; set; }
        public string SocialLink { get; set; }
    }
}
