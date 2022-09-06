using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.ViewModels.Account
{
    public class BoolResultVM
    {
        public bool IsSuccessed { get; set; }
        public string StatusCode { get; set; }
        public string Data { get; set; }
    }
    public class UserListResponse
    {
        public UserListResponse()
        {
            this.Data = new List<UpdateUserVM>();
        }
        public string StatusCode { get; set; }

        public string Error { get; set; }
        public  List<UpdateUserVM> Data { get; set; }
    }
    public class UserProfileResponse
    {
        public bool IsSuccessed { get; set; }
        public string StatusCode { get; set; }

        public string Error { get; set; }
        public UpdateUserVM UserProfileData { get; set; }
    }
    public class StatusCodeVM
    {
        public string StatusCode { get; set; }
    }
}
