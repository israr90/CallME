using foneMe.ViewModels.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace foneMe.SL.Interface
{
  
    public interface IUserAboutmeRepository : IRepository<UserAboutMeViewModel>
    {
        void UpdateUserAboutme(UserAboutMeViewModel model);
        UserAboutMeViewModel GetUserAboutMe(String UserID);
        List<UserAboutMeViewModel> GetMatchingAboutme(String Profession);
        Boolean AddUserAboutme(UserAboutMeViewModel model);
        Boolean AddUserAboutMeSocialLink(List<UserSocialLink> links);
        Boolean DeleteUserAboutMeSocialLink(String Id);
        Boolean UpdateUserAboutMeSocialLink(String Id, String Name, String SocialLink);
        List<UserAboutMeViewModel> UserNearMe(Double Latitude, Double Longitude, Double Radius);

        Boolean UpdateUserLocation(Double Latitude, Double Longitude, String UserID);
    }
}
