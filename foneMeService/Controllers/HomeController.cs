using foneMe.DAL;
using foneMe.SL.Interface;
using foneMe.ViewModels.Model;
using foneMeService.Identity;
using foneMeService.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace foneMeService.Controllers
{

    public class HomeController : Controller
    {
        private UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString);


        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public ActionResult UserData()
        {
            ViewBag.Title = "User Profile";

            return View();
        }

        //[Route("Home/AboutMe/{*foneme}")]
        public ActionResult AboutMe(String foneme)
        {

            ViewBag.data = new List<UserAboutMeViewModel>();
            ViewBag.Title = "About Me";
            
            if (foneme != "" && foneme != null && foneme != "[^\\.]*")
            {

                var user = _unitOfWork.UserRepository.FindByCNIC(foneme);

                if (user != null)
                {
                    string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                    ViewBag.isFound = "true";
                    ViewBag.UserId = user.UserId;
                    ViewBag.name = user.FirstName + " " + user.LastName;
                    ViewBag.foneme = "fone.me/" + foneme;
                    //ViewBag.phone = user.LoginName;

                    if (user.ImageURL != null && user.ImageURL != "")
                    {
                        ViewBag.ImageURL = ProjectImgPth + user.ImageURL;
                    }
                    else
                    {
                        ViewBag.ImageURL = ProjectImgPth + "person.jpeg";
                    }
                    var aboutMe = _unitOfWork.UserAboutmeRepository.GetUserAboutMe(user.UserId.ToString());
                   
                    if (aboutMe != null)
                    {
                        ViewBag.aboutme = aboutMe.AboutMe;
                        ViewBag.profession = aboutMe.Profession;
                        ViewBag.address = aboutMe.Address;
                        ViewBag.links=aboutMe.UserAboutMeLink;
                    }

                   

                }
                else
                {
                    ViewBag.isFound = "false";
                }
                return View();
            }
            else
            {
                ViewBag.isFound = "";

                return View();
                // return RedirectToAction("Index","Home");

            }
        }

        public ActionResult G()
        {
            ViewBag.Title = "Group Channel";

            return View();
        }

        //public ActionResult Freelancers(String foneme)
        [HttpGet]
        //[ValidateAntiForgeryToken]
        public ActionResult Freelancers(String searchValue)
        {

            //ViewBag.isFound = "false";
            ViewBag.data = new List<UserAboutMeViewModel>();
            if (searchValue != "" && searchValue != null)
            {
                ViewBag.Title = "Freelancers";
                string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                var freelancers = _unitOfWork.UserAboutmeRepository.GetMatchingAboutme(searchValue);
                freelancers.All(x =>
                {
                    if (x.ImageURL != null && x.ImageURL != "")
                    {
                        x.ImageURL = ProjectImgPth + x.ImageURL;
                    }
                    else
                    {
                        x.ImageURL = ProjectImgPth + "person.jpeg";
                    }

                    return true;
                });
                ViewBag.data = freelancers;
            }
            return View("Freelancers", new { searchValue = searchValue });
        }



        public ActionResult ContactUser(String id)
        {
            ContactFormViewModel viewModel = new ContactFormViewModel();
            viewModel.ReceieverUserId = id;
            ViewBag.Title = "Contact User";
            ViewBag.data = "";

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SendMessage(String ReceieverUserId, String code, String FullName, String SenderMobileNumber, String Message)
        {
            try
            {
                var uId = new Guid(ReceieverUserId);
                ViewBag.data = "";
                var user = _unitOfWork.UserRepository.FindById(uId);
                if (user != null)
                {
                    var ReceieverMobileNumber = user.LoginName;




                    //  Find your Account Sid and Token at twilio.com / console
                    //   DANGER! This is insecure.See http://twil.io/secure
                    //const string accountSid = "AC698df3ece05eadb3faf0cf481dfdc221";
                    //const string authToken = "44d9c8ea97863c9ee856c7e29aa79da7";

                    string accountSid = ConfigurationManager.AppSettings["TwilioOTPSID"].ToString();
                    string authToken = ConfigurationManager.AppSettings["TwilioOTPToken"].ToString();


                    TwilioClient.Init(accountSid, authToken);
                    var message = MessageResource.Create(

                        body: "Message From: " + FullName + "\n\nMessage\n" + Message + "\n\nContact Number  :\n" + code + SenderMobileNumber,
                        from: new Twilio.Types.PhoneNumber("+12027604749"),
                        to: new Twilio.Types.PhoneNumber(ReceieverMobileNumber)
                    );


                    ViewBag.data = "Message Sent Successfully!";


                }

            }
            catch (Exception ex)
            {
                ViewBag.data = "Message Sending Failed!";
            }

            ContactFormViewModel viewModel = new ContactFormViewModel();
            viewModel.ReceieverUserId = ReceieverUserId;

            return View("ContactUser", viewModel);
        }

        public ActionResult Error()
        {
            String url = Request.Url.AbsoluteUri;
            return View();
        }


    }
}
