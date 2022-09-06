using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.ViewModels.Account
{
    public class ReviewsVM
    {
        public string UserImage { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public string CreatedDate { get; set; }
    }
}
