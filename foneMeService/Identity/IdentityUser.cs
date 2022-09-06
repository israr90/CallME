using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace foneMeService.Identity
{
    public class IdentityUser : IUser<Guid>
    {
        public IdentityUser()
        {
            Id = Guid.NewGuid();
            Update = false;
        }

        public IdentityUser(string userName) : this()
        {
            UserName = userName;
        }

        public string UserName { get; set; }
        public Guid Id { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public string FirstName { get; set; }
        public string FullName { get; set; }
        public string FatherName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string PhoneNo { get; set; }
        public string Email { get; set; }
        public string CNIC { get; set; }
        public string Gender { get; set; }
        public string CountryCode { get; set; }
        public string NumberWithOutCode { get; set; }
        public bool Update { get; set; }
        public string RoleName { get; set; }
        public DateTime DateOfBirth { get; internal set; }
        public List<foneMe.ViewModels.ContactViewModel> Contacts { get; set; }
        public long AreaId { get; internal set; }
    }
}