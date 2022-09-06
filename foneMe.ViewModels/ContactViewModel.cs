using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.ViewModels
{
    public class ContactViewModel
    {
        public long ContactId { get; set; }
        public long ContactCategoryProfileId { get; set; }
        public long UserId { get; set; }
        public long BrandLocationId { get; set; }
        public long ContactTypeProfileId { get; set; }
        public string Description { get; set; }
        public string ContactCategoryName { get; set; }
        public string ContactTypeName { get; set; }
    }

    public class CustomerContactViewModel
    {
        public long ContactId { get; set; }
        public string Description { get; set; }
    }

    public class PostContactViewModel
    {
        public string Description { get; set; }
    }
}
