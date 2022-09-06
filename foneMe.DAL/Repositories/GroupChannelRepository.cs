using Dapper;
using foneMe.SL.Entities;
using foneMe.SL.Interface;
using foneMe.ViewModels.Account;
using foneMe.ViewModels.Model;
using foneMe.ViewModels.Twilio;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.DAL.Repositories
{

    internal class GroupChannelRepository : Repository<CallConnection>, IGroupChannelRepository
    {
        private readonly ApplicationDbContext _context;
        internal GroupChannelRepository(ApplicationDbContext context)
            : base(context)
        { _context = context; }

        public void AddGroup(GroupDetailsViewModel model)
        {
                var Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;

            using (var connection = new SqlConnection(Connection))
            {
                connection.Open();
                connection.Query("insert into GroupDetails(GroupID,UserID,GroupName,GroupDescription,GroupLink,IsPublic,IsGroup,PublicGroupLink,DeepLink) values(@GroupID,@UserID,@GroupName,@GroupDescription,@GroupLink,@IsPublic,@IsGroup,@PublicGroupLink,@DeepLink)",
                    new { GroupID = model.GroupID, UserID =model.UserID, GroupName= model.GroupName, GroupDescription= model.GroupDescription, GroupLink= model.GroupLink, IsPublic= model.IsPublic, IsGroup= model.IsGroup, PublicGroupLink = model.PublicGroupLink, DeepLink=model.DeepLink });
                connection.Close();
            }
        }

        public void UpdateGroupInfo(GroupDetailsViewModel model)
        {
            var Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;

            using (var connection = new SqlConnection(Connection))
            {
                connection.Open();
                connection.Execute("Update GroupDetails set UserID = @UserID, GroupName = @GroupName, GroupDescription = @GroupDescription,  GroupLink = @GroupLink, IsPublic = @IsPublic, IsGroup=@IsGroup, PublicGroupLink=@PublicGroupLink, DeepLink=@DeepLink where GroupID = @GroupID",
                new {UserID = model.UserID, GroupName = model.GroupName, GroupDescription = model.GroupDescription, GroupLink = model.GroupLink, IsPublic = model.IsPublic, IsGroup = model.IsGroup, PublicGroupLink = model.PublicGroupLink, DeepLink = model.DeepLink, GroupID = model.GroupID });
                connection.Close();
            }


        }
        public List<GroupDetailsViewModel> GetSingleGroup(GroupDetailsViewModel model)
        {

            List<GroupDetailsViewModel> groups = new List<GroupDetailsViewModel>();
            var Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;


            using (var connection = new SqlConnection(Connection))
            {
                connection.Open();
                groups = connection.Query<GroupDetailsViewModel>("Select * from GroupDetails where GroupID=@GroupID", new { GroupID = model.GroupID }).ToList();
                connection.Close();
            }

            return groups;
        }

        public List<GroupDetailsViewModel> GetGroupByName(GroupDetailsViewModel model)
        {

            List<GroupDetailsViewModel> groups = new List<GroupDetailsViewModel>();
            var Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;


            using (var connection = new SqlConnection(Connection))
            {
                connection.Open();
                groups = connection.Query<GroupDetailsViewModel>("Select * from GroupDetails where GroupName=@GroupName", new { GroupName = model.GroupName }).ToList();
                connection.Close();
            }
            return groups;
        }

        public GroupDetailsViewModel GetGroupByDeepLink(GroupDetailsViewModel model)
        {

            GroupDetailsViewModel group = new GroupDetailsViewModel();
            var Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;


            using (var connection = new SqlConnection(Connection))
            {
                connection.Open();
                group = connection.Query<GroupDetailsViewModel>("Select * from GroupDetails where DeepLink=@DeepLink", new { DeepLink = model.DeepLink }).FirstOrDefault();
                connection.Close();
            }
            return group;
        }

        public GroupDetailsViewModel GetGroupByURL(GroupDetailsViewModel model)
        {

            GroupDetailsViewModel group = new GroupDetailsViewModel();
            var Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;


            using (var connection = new SqlConnection(Connection))
            {
                connection.Open();
                group = connection.Query<GroupDetailsViewModel>("Select * from GroupDetails where GroupLink=@GroupLink", new { GroupLink = model.GroupLink }).FirstOrDefault();
                connection.Close();
            }

            return group;
        }
        public List<GroupDetailsViewModel> GetAllGroups(GroupDetailsViewModel model)
        {
            List<GroupDetailsViewModel> groups = new List<GroupDetailsViewModel>();
                var Connection = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;


            using (var connection = new SqlConnection(Connection))
            {
                connection.Open();
                groups = connection.Query<GroupDetailsViewModel>("Select * from GroupDetails where UserID=@UserID", new { UserID = model.UserID.ToString() }).ToList();
                connection.Close();
            }

            return groups;

            
        }


    }
}
