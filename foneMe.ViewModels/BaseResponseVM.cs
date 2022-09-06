using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.ViewModels
{
    public class BaseResponseVM<T> where T : class
    {
        public string StatusCode { get; set; }
        public string Message { get; set; }
        public long TotalRecords { get; set; }
        public IEnumerable<T> Data { get; set; }
    }
}
