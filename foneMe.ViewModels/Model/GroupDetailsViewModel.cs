using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.ViewModels.Model
{
    
    public class GroupDetailsViewModel
    {
        public int Id { get; set; }
        public string GroupID { get; set; }

        //  [Required]
        // public Guid UserID { get; set; }
        public String UserID { get; set; }

    //    [Required]
        public string GroupName { get; set; }
        public string DeepLink { get; set; }

        [Required]
        public string GroupDescription { get; set; }

  //      [Required]
        public string GroupLink { get; set; }

     //   [Required]
        public string IsPublic { get; set; }

   //     [Required]
        public string IsGroup { get; set; }
        public string PublicGroupLink { get; set; }
    }

    public class GroupDetailsResponse
    {
        public string StatusCode { get; set; }
        public string Message { get; set; }

        public List<GroupDetailsViewModel> GroupData { get; set; }

    }
}
