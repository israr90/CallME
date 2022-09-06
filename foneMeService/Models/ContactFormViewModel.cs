using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace foneMeService.Models
{
    public class ContactFormViewModel
    {
       
        public  String ReceieverUserId;
      
        public String SenderMobileNumber;
      
        public String ReceieverMobileNumber;
       
        public String SenderName;
     
        public String Message;
    }
}