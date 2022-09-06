using Dapper;
using foneMe.SL.Interface;
using foneMe.ViewModels.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;

namespace foneMe.DAL.Repositories
{
    //class UserAboutmeRepository

    internal class UserAboutmeRepository : Repository<UserAboutMeViewModel>, IUserAboutmeRepository
    {
        //String Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;
        private readonly ApplicationDbContext _context;
        internal UserAboutmeRepository(ApplicationDbContext context)
            : base(context)
        { _context = context; }

        public void UpdateUserAboutme(UserAboutMeViewModel model)
        {
            var Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;
            using (var connection = new SqlConnection(Connection))
            {
                connection.Open();
                connection.Execute("Update UserAboutme set  PhoneModel = @PhoneModel, PhoneBrand = @PhoneBrand, Profession=@Profession, Address=@Address, AboutMe = @AboutMe where UserID = @UserID",
                new { PhoneModel = model.PhoneModel, PhoneBrand = model.PhoneBrand, Profession = model.Profession, Address = model.Address, AboutMe = model.AboutMe, UserID = model.UserID, });
                connection.Close();
            }

        }
        public UserAboutMeViewModel GetUserAboutMe(String UserID)
        {
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;
            UserAboutMeViewModel userAboutMeViewModel = new UserAboutMeViewModel();
            using (var connection = new SqlConnection(Connection))
            {
                connection.Open();
                //var list = connection.Query("Select * from UserSocialLink where UserID=@UserID", new { UserID = UserID }).ToList();
                var links = connection.Query<UserSocialLink>("Select * from UserSocialLink where UserID=@UserID", new { UserID = UserID }).ToList();
                userAboutMeViewModel = connection.Query<UserAboutMeViewModel>("Select * from UserAboutme where UserID=@UserID", new { UserID = UserID }).FirstOrDefault();
                if (userAboutMeViewModel != null)
                {

                    userAboutMeViewModel.UserAboutMeLink = links;
                }
                connection.Close();
            }
            return userAboutMeViewModel;

        }
        public Boolean AddUserAboutme(UserAboutMeViewModel model)
        {
            try
            {
                var Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;
                using (var connection = new SqlConnection(Connection))
                {
                    connection.Open();
                    connection.Query("Insert into UserAboutme(UserID,PhoneModel,PhoneBrand,Profession,Address,AboutMe) values(@UserID,@PhoneModel,@PhoneBrand, @Profession, @Address, @AboutMe)",
                        new { UserID = model.UserID, PhoneModel = model.PhoneModel, PhoneBrand = model.PhoneBrand, Profession = model.Profession, Address = model.Address, AboutMe = model.AboutMe });
                    connection.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
      
        public Boolean AddUserAboutMeSocialLink(List<UserSocialLink> links)
        {
            try
            {
                var Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;
                using (var connection = new SqlConnection(Connection))
                {
                    connection.Open();
                    foreach (var model in links)
                    {
                        
                        //var aboutMeLink = connection.Query("Select * from UserSocialLink where UserID=@UserID AND @SocialLink=@SocialLink", new { UserID = model.UserID, SocialLink = model.SocialLink }).FirstOrDefault();
                        var aboutMeLink = connection.Query<UserSocialLink>("Select * from UserSocialLink where UserID='"+model.UserID+"' AND SocialLink='"+model.SocialLink+"'").FirstOrDefault();
                        if (aboutMeLink==null)
                        {
                            connection.Query("Insert into UserSocialLink(UserID,Name,SocialLink) values(@UserID,@Name,@SocialLink)",
                                new { UserID = model.UserID, Name = model.Name, SocialLink = model.SocialLink });
                        }
                      
                    }
                    connection.Close();

                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public Boolean DeleteUserAboutMeSocialLink(String Id)
        {
            try
            {
                var Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;
                using (var connection = new SqlConnection(Connection))
                {
                    connection.Open();
                    connection.Query("Delete from UserSocialLink where Id=@Id", new { Id = Id });
                    connection.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public Boolean UpdateUserAboutMeSocialLink(String Id,String Name, String SocialLink)
        {
            try
            {
                var Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;
                using (var connection = new SqlConnection(Connection))
                {
                    connection.Open();
                    connection.Query("Update UserSocialLink set Name=@Name, SocialLink=@SocialLink where Id=@Id", new { Name= Name, SocialLink = SocialLink, Id = Id });
                    connection.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public List<UserAboutMeViewModel> GetMatchingAboutme(String Profession)
        {
            Profession = Profession.Replace(' ', '%');
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;
            List<UserAboutMeViewModel> userAboutMeViewModel = new List<UserAboutMeViewModel>();
            using (var connection = new SqlConnection(Connection))
            {
                connection.Open();
                //userAboutMeViewModel = connection.Query<UserAboutMeViewModel>("Select * from UserAboutme where Profession=@Profession", new { Profession = Profession }).ToList();
                // userAboutMeViewModel = connection.Query<UserAboutMeViewModel>("Select * from UserAboutme where Profession LIKE CONCAT('%',@Profession,'%');", new { Profession = Profession }).ToList();
                //select Users.FirstName,Users.LastName,Users.ImageURL,Users.CNIC as FoneMe,UserAboutme.*  from Users,UserAboutme  WHERE Users.UserId=UserAboutme.UserID and UserAboutme.Profession like '%Developer%'  

                //select Users.FirstName,Users.LastName,Users.ImageURL,Users.CNIC as FoneMe,UserAboutme.* from Users Left JOIN UserAboutme on Users.UserId = UserAboutme.UserID 
                //WHERE Users.FirstName LIKe '%mobile%' or Users.CNIC LIKe '%mobile%' or UserAboutme.Profession LIKe '%mobile%' or UserAboutme.Address LIKe '%mobile%' or UserAboutme.AboutMe LIKe '%mobile%'

                userAboutMeViewModel = connection.Query<UserAboutMeViewModel>("select Users.FirstName,Users.LastName,Users.ImageURL,Users.CNIC as FoneMe,UserAboutme.* " +
                    "from Users Left JOIN UserAboutme on Users.UserId = UserAboutme.UserID " +
                    "WHERE Users.FirstName LIKe CONCAT('%',@FirstName,'%') or " +
                    "Users.CNIC LIKE CONCAT('%',@CNIC,'%') or " +
                    "UserAboutme.Profession LIKE CONCAT('%',@Profession,'%')  or " +
                    "UserAboutme.Address LIKE CONCAT('%',@Address,'%') or " +
                    "UserAboutme.AboutMe LIKE CONCAT('%',@AboutMe,'%');", 

                    new {
                        FirstName = Profession,
                        CNIC = Profession,
                        Profession = Profession,
                        Address = Profession,
                        AboutMe = Profession
                    }).ToList();

                connection.Close();
            }

            return userAboutMeViewModel;

        }


        public List<UserAboutMeViewModel> UserNearMe(Double Latitude,Double Longitude,Double Radius)
        {
            List<UserAboutMeViewModel> userAboutMeViewModel = new List<UserAboutMeViewModel>();
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;
            string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
            using (var connection = new SqlConnection(Connection))
            {
                connection.Open();
               
                userAboutMeViewModel = connection.Query<UserAboutMeViewModel>("exec UserNearMe @LAT1=@LAT1, @LONG1=@LONG1, @RAD1=@RAD1",
                    new
                    {
                        LAT1 = Latitude,
                        LONG1 = Longitude,
                        RAD1 =Radius
                    }).ToList();

                connection.Close();
            }
            userAboutMeViewModel.ForEach(s => s.ImageURL = s.ImageURL==null? null : ProjectImgPth + s.ImageURL);

            return userAboutMeViewModel;

        }

        public Boolean UpdateUserLocation(Double Latitude, Double Longitude, String UserID)
        {
          
            string Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;
            using (var connection = new SqlConnection(Connection))
            {
                connection.Open();
                connection.Query("Update UserAboutme set Latitude=@Latitude, Longitude=@Longitude where UserID=@UserID", new { Latitude=Latitude, Longitude = Longitude, UserID = UserID });
                connection.Close();
            }
            return true;

        }

    }
}
