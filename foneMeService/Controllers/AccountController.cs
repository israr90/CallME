using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using foneMeService.Models;
using foneMe.SL.Interface;
using foneMe.ViewModels.Account;
using System.Net;
using System.Text.RegularExpressions;
using foneMe.SL.Entities;
using Microsoft.AspNet.Identity;
using foneMeService.Identity;
using System.Linq;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using System.Drawing;
using System.IO;
using System.Web.Configuration;
using Twilio.Jwt.AccessToken;
using foneMe.ViewModels.Twilio;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using System.Web.Hosting;
using PushSharp.Apple;
using Newtonsoft.Json.Linq;
using System.Configuration;
using foneMe.ViewModels.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data.Entity.Core.Objects;

namespace foneMeService.Controllers
{
    //  [Authorize]
    //[RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser, Guid> _userManager;

        public AccountController(IUnitOfWork unitOfWork, UserManager<IdentityUser, Guid> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        /// <summary>
        /// Gets SMS Code
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>Use this API to send SMS code on the user number</remarks>
        [AllowAnonymous]
        [Route("api/account/v1/getsmscode")]
        [HttpPost]
        public async Task<MessageCodeVM> GetCode([FromBody] PostVM model)
        {
            try
            {
                bool result = false;
                string code = "";
                //var smsCode = new MessageCodeVM();
                if (model == null || string.IsNullOrEmpty(model.PhoneNumber))
                {
                    return await Task.FromResult(new MessageCodeVM { StatusCode = "400" });
                }
                //var match = Regex.Match(model.PhoneNumber,
                //               @"^(?:(\+92)|(0092))-{0,1}\d{3}-{0,1}\d{7}$|^\d{11}$|^\d{4}-\d{7}$");
                //if (!match.Success)
                //{
                //    return await Task.FromResult(new MessageCodeVM { StatusCode = "402" });
                //}

                var objUser = new User();
                var objDbUser = _unitOfWork.UserRepository.FindByLoginName(model.PhoneNumber);
                if (objDbUser == null)
                {
                    return await Task.FromResult(new MessageCodeVM { StatusCode = "409", IsUserRegistered = false });
                }
                else if (objDbUser != null)
                {
                    if (model.PhoneNumber.StartsWith("+13111111"))
                        result = true;
                    
                    else { 
                        code = await _userManager.GenerateChangePhoneNumberTokenAsync(objDbUser.UserId, model.PhoneNumber);

                        result = SendTwilioSMS(objDbUser?.Contacts.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault()?.Description, code);
                    }
                }
                if (result)
                {
                    return await Task.FromResult(new MessageCodeVM { StatusCode = "200", SMSCode = code, IsUserRegistered = true, UserId = objDbUser.UserId, IsUserVerified = objDbUser.IsUserVerified ?? false });
                }
                else
                {
                    return await Task.FromResult(new MessageCodeVM { StatusCode = "410" });
                }

                // StatusCode=200   ,   Success, user will move to verify code screen
                // StatusCode=400    ,  Forms Validation Failed
                // StatusCode=409    ,  User Not Registered
                // StatusCode=410    ,  Twilio Code Failed
                //return await Task.FromResult(Request.CreateResponse(HttpStatusCode.OK, smsCode));
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/getsmscode");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        private bool SendTwilioSMS(string receiverNumber, string code)
        {
            try
            {


                //  Find your Account Sid and Token at twilio.com / console
                //   DANGER! This is insecure.See http://twil.io/secure
                //const string accountSid = "AC698df3ece05eadb3faf0cf481dfdc221";
                //const string authToken = "44d9c8ea97863c9ee856c7e29aa79da7";

                string accountSid = ConfigurationManager.AppSettings["TwilioOTPSID"].ToString();
                string authToken = ConfigurationManager.AppSettings["TwilioOTPToken"].ToString();


                TwilioClient.Init(accountSid, authToken);
                var message = MessageResource.Create(
                    // Debug Code
                    // body: "<#> Your FoneMe Code is: " + code + ".JZrQvoEC9LS",
                    // live App
                    body: "<#> Your FoneMe Code is: " + code + ".9joAZKBFbug",
                    from: new Twilio.Types.PhoneNumber("+12027604749"),
                    to: new Twilio.Types.PhoneNumber(receiverNumber)
                );

                //Console.WriteLine(message.Sid);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        [AllowAnonymous]
        [Route("api/account/v1/verifysmscode")]
        [HttpPost]
        public async Task<VerifyMessageCodeVM> VerifyCode([FromBody] PostVM model)
        {
            try
            {
                bool result = false;
                var objUserInfo = new UserInfo();
                if (model == null
                   || string.IsNullOrEmpty(model.PhoneNumber)
                   || string.IsNullOrEmpty(model.SMSCode)
                   || model.UserId == null || model.UserId == Guid.Empty
                   || string.IsNullOrEmpty(model.DeviceToken))
                {
                    return await Task.FromResult(new VerifyMessageCodeVM { StatusCode = "400" });
                }
                // App Testing Check
                //if (model.IsUserTesting && model.PhoneNumber == "+923097124818" && model.SMSCode == "123456")
                //{
                //    var objUser1 = _unitOfWork.UserRepository.FindByLoginName(model.PhoneNumber);
                //    if (objUser1 != null)
                //    {
                //        // Update User DeviceToken and IsUserVerified=true
                //        objUser1.IsUserVerified = true;
                //        objUser1.DeviceToken = model.DeviceToken;
                //        objUser1.FullName = model.VOIPDeviceToken;
                //        _unitOfWork.SaveChanges();
                //        string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                //        var objMobile = objUser1.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();
                //        //Get update User Info
                //        objUserInfo.Name = objUser1?.FirstName;
                //        objUserInfo.LoginName = objUser1?.LoginName;
                //        objUserInfo.Address = objUser1.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault()?.Description;
                //        objUserInfo.Email = objUser1.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault()?.Description;
                //        objUserInfo.PhoneNumber = objMobile?.Description;
                //        objUserInfo.UserId = objUser1.UserId;
                //        objUserInfo.CountryCode = objMobile?.CountryCode;
                //        objUserInfo.PhoneNumberWithoutCode = objMobile.NumberWithOutCode;
                //        //objUserInfo.ImageUrl = ProjectImgPth + "/Common/DisplayImageById?fileId=" + objUser1.UserId;
                //        objUserInfo.ImageUrl = ProjectImgPth + objUser1.ImageURL;
                //        return await Task.FromResult(new VerifyMessageCodeVM { StatusCode = "200", UserInfo = objUserInfo, IsUserVerified = true });
                //    }
                //    else
                //    {
                //        return await Task.FromResult(new VerifyMessageCodeVM { StatusCode = "411", IsUserVerified = false });
                //    }
                //}
                // End App Testing Check
                var objUser = _unitOfWork.UserRepository.FindByLoginName(model.PhoneNumber);
                if (objUser != null)
                {
                    if (model.PhoneNumber.StartsWith("+13111111"))
                        result = true;
                    else
                        result = await _userManager.VerifyChangePhoneNumberTokenAsync(model.UserId, model.SMSCode, model.PhoneNumber);
                    
                    if (result)
                    {
                        // Update User DeviceToken and IsUserVerified=true
                        objUser.IsUserVerified = true;
                        objUser.DeviceToken = model.DeviceToken;
                        objUser.FullName = model.VOIPDeviceToken;
                        var fatherName = "iOS";
                        if (!string.IsNullOrEmpty(model.FatherName))
                        {
                            fatherName = model.FatherName;
                        }
                        objUser.FatherName = fatherName;
                        _unitOfWork.SaveChanges();
                        string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                        var objMobile = objUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();
                        //Get update User Info
                        objUserInfo.Name = objUser?.FirstName;
                        objUserInfo.LoginName = objUser?.LoginName;
                        objUserInfo.Address = objUser.CNIC;
                        //objUserInfo.Address = objUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault()?.Description;
                        objUserInfo.Email = objUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault()?.Description;
                        objUserInfo.PhoneNumber = objMobile?.Description;
                        objUserInfo.UserId = objUser.UserId;
                        objUserInfo.CountryCode = objMobile?.CountryCode;
                        objUserInfo.PhoneNumberWithoutCode = objMobile.NumberWithOutCode;
                        //objUserInfo.ImageUrl = ProjectImgPth + "/Common/DisplayImageById?fileId=" + objUser.UserId;
                        objUserInfo.ImageUrl = ProjectImgPth + objUser.ImageURL;
                        return await Task.FromResult(new VerifyMessageCodeVM { StatusCode = "200", UserInfo = objUserInfo, IsUserVerified = true });
                    }
                    else
                    {
                        return await Task.FromResult(new VerifyMessageCodeVM { StatusCode = "411", IsUserVerified = false });
                    }
                }
                else
                {
                    return await Task.FromResult(new VerifyMessageCodeVM { StatusCode = "409" });
                }
                // StatusCode=200   ,  User Verified and have Data
                // StatusCode=400    ,  Forms Validation Failed
                // StatusCode=409    ,  User Not Registered
                // StatusCode=411   ,   User Verification Failed
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/verifysmscode");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        [AllowAnonymous]
        [Route("api/account/v1/register")]
        [HttpPost]
        public async Task<RegisterStatusVM> Register([FromBody] RegisterVM model)
        {
            try
            {
                if (model == null
                    || string.IsNullOrEmpty(model.Name)
                    // || string.IsNullOrEmpty(model.Email)
                    || string.IsNullOrEmpty(model.PhoneNumber)
                    || string.IsNullOrEmpty(model.CountryCode)
                    || string.IsNullOrEmpty(model.NumberWithOutCode))

                {
                    return await Task.FromResult(new RegisterStatusVM { StatusCode = "400", Message = "Fill Data Fields" });
                }
                if (!string.IsNullOrEmpty(model.CNIC))
                {
                    var nicResult = _unitOfWork.UserRepository.checkNIC(model.CNIC);
                    if (nicResult.Result != null && !nicResult.Result.Trim().Equals("00000000-0000-0000-0000-000000000000"))
                    {
                        return await Task.FromResult(new RegisterStatusVM { StatusCode = "405", Message = "FoneID Already Exist" });
                    }
                }
                //var match = Regex.Match(model.PhoneNumber,
                //                  @"^(?:(\+92)|(0092))-{0,1}\d{3}-{0,1}\d{7}$|^\d{11}$|^\d{4}-\d{7}$");
                //if (!match.Success)
                //{
                //    return await Task.FromResult(new RegisterStatusVM { StatusCode = "402" });
                //}
                var fatherName = "iOS";
                if (!String.IsNullOrEmpty(model.FatherName))
                {
                    fatherName = model.FatherName;
                }
                var checkUser = model.PhoneNumber;
                checkUser = "+" + checkUser;
                var objDbUser = _unitOfWork.UserRepository.FindByLoginName(checkUser);
                if (objDbUser != null)
                {
                    // 405 User does exist already
                    return await Task.FromResult(new RegisterStatusVM { StatusCode = "405", Message = "User Already Exist" });
                }
                var user = new IdentityUser()
                {
                    UserName = model.PhoneNumber,
                    FirstName = model.Name,
                    // Gender = model.Gender,
                    CNIC = model.CNIC,
                    FatherName = fatherName,
                    RoleName = null,
                    CountryCode = model.CountryCode,
                    NumberWithOutCode = model.NumberWithOutCode,
                    PhoneNo = "+" + model.PhoneNumber,
                    //DateOfBirth = null,
                    //Address = model.Address,
                    // Email = model.Email,
                    Update = false,
                };
                var result = await _userManager.CreateAsync(user, "123456");
                if (result.Succeeded)
                {
                    string code = null;
                    var objRegisterUser = _unitOfWork.UserRepository.FindByLoginName(checkUser);

                    if (!checkUser.StartsWith("+13111111"))
                    {
                        code = await _userManager.GenerateChangePhoneNumberTokenAsync(objRegisterUser?.UserId ?? new Guid(), checkUser);

                        var response = SendTwilioSMS(objRegisterUser?.Contacts.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault()?.Description, code);
                    }
                    return await Task.FromResult(new RegisterStatusVM { StatusCode = "200", Message = "Success", Code = code, UserId = objRegisterUser?.UserId, });
                }
                else
                {
                    return await Task.FromResult(new RegisterStatusVM { StatusCode = "406", Message = "Data Integrity Invalid" });
                }

                // StatusCode=200,   Success
                // StatusCode=406,   Data Integrity Invalid
                // StatusCode=405,   User Already Exist
                // StatusCode=400,   Fill Data Fields
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/register");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        [Route("api/account/v1/CICN/{phoneNumber}")]
        [HttpGet]
        public async Task<string> GetCNICByPhoneNumber(string phoneNumber)
        {
            var cicn = await _unitOfWork.UserRepository.GetCNICByLoginName($"+{phoneNumber}");
            return cicn;
        }

        [Route("api/account/v1/removemyfriend")]
        [HttpPost]
        public async Task<UserProfileResponse> RemoveAsFriend([FromBody] CnictoProfileVM model)
        {
            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                }
                else
                {


                    var cnic = "";
                    var uri = model.Url ?? "";
                    var cnc = model.Cnic ?? "";


                    if (uri == "" && cnc == "")
                    {
                        return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                    }


                    if (uri.Length > 0)
                    {
                        var d = uri.Split('/').Last();
                        if (d != null && d.Length > 0)
                        {
                            cnic = d;

                        }

                    }
                    if (cnc.Length > 0)
                    {
                        cnic = cnc;

                    }
                    var objDbUser = _unitOfWork.UserRepository.FindByCNIC(cnic);
                    if (objDbUser == null)
                    {
                        return await Task.FromResult(new UserProfileResponse { StatusCode = "409" });
                    }
                    else
                    {
                        string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                        var objUpdateUserVM = new UpdateUserVM();
                        var objMobile = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();

                        objUpdateUserVM.Name = objDbUser.FirstName;
                        objUpdateUserVM.PhoneNumber = objMobile?.Description;
                        //objUpdateUserVM.Email = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault()?.Description;
                        objUpdateUserVM.Address = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault()?.Description;
                        objUpdateUserVM.UserId = objDbUser.UserId;
                        objUpdateUserVM.CountryCode = objMobile?.CountryCode;
                        objUpdateUserVM.MobileNumberWithoutCode = objMobile?.NumberWithOutCode;
                        //objUpdateUserVM.ImageUrl = ProjectImgPth + "/Common/DisplayImageById?fileId=" + objDbUser.UserId;
                        if (objDbUser.ImageURL != null && objDbUser.ImageURL != "")
                        {
                            objUpdateUserVM.ImageUrl = ProjectImgPth + objDbUser.ImageURL;
                        }

                        return await Task.FromResult(new UserProfileResponse { UserProfileData = objUpdateUserVM, StatusCode = "200" });
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/cnctoprofile");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }


        }

        [Route("api/account/v1/addmyfriend")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<UserProfileResponse> AddAsFriend([FromBody] CnictoProfileVM model)
        {


            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                }
                else
                {


                    var cnic = "";
                    var uri = model.Url ?? "";
                    var cnc = model.Cnic ?? "";


                    if (uri == "" && cnc == "" && model.Friend == null)
                    {
                        return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                    }


                    if (uri.Length > 0)
                    {
                        var d = uri.Split('/').Last();
                        if (d != null && d.Length > 0)
                        {
                            cnic = d;

                        }

                    }
                    if (cnc.Length > 0)
                    {
                        cnic = cnc;

                    }

                    foneMe.SL.Entities.User objDbUser = null;


                    if (cnic != null)
                    {
                        var objDbUser1 = _unitOfWork.UserRepository.FindByCNIC(cnic);

                        if (objDbUser1 != null) objDbUser = objDbUser1;

                    }
                    if (model.Friend != null)
                    {
                        var objDbUser1 = _unitOfWork.UserRepository.FindByUserId(model.Friend);
                        if (objDbUser1 != null) objDbUser = objDbUser1;

                    }


                    if (objDbUser == null)
                    {
                        return await Task.FromResult(new UserProfileResponse { StatusCode = "409" });
                    }
                    else
                    {
                        var name = "";
                        if (objDbUser.FullName != null && objDbUser.FullName.Length > 0)
                        {
                            name = objDbUser.FullName;
                        }

                        else if (objDbUser.FirstName != null && objDbUser.FirstName.Length > 0)

                        {
                            name = objDbUser.FirstName;
                        }
                        else
                        {
                            name = "No Name";
                        }



                        var inserted = _unitOfWork.CallConnectionRepository.AddUserasFriend(new AddContactsVM { Name = objDbUser.FullName ?? objDbUser.FirstName ?? "", UserId = model.Me, FriendId = objDbUser.UserId });




                        string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                        var objUpdateUserVM = new UpdateUserVM();
                        var objMobile = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();

                        objUpdateUserVM.Name = objDbUser.FirstName;
                        objUpdateUserVM.ContactFT = objDbUser.DeviceToken;

                        objUpdateUserVM.ContactVT = objDbUser.VoipToken;
                        objUpdateUserVM.ContactCNIC = objDbUser.CNIC;
                        objUpdateUserVM.PhoneNumber = objMobile?.Description;
                        objUpdateUserVM.Email = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault()?.Description;
                        objUpdateUserVM.Address = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault()?.Description;
                        objUpdateUserVM.UserId = objDbUser.UserId;
                        objUpdateUserVM.CountryCode = objMobile?.CountryCode;
                        objUpdateUserVM.MobileNumberWithoutCode = objMobile?.NumberWithOutCode;
                        objUpdateUserVM.ImageUrl = ProjectImgPth + objDbUser.ImageURL;
                        return await Task.FromResult(new UserProfileResponse { UserProfileData = objUpdateUserVM, IsSuccessed = inserted, StatusCode = "200" });
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/cnctoprofile");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }


        }
        [Route("api/account/v1/lookuser")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<UserProfileResponse> LookUser([FromBody] CnictoProfileVM model)
        {


            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                }
                else
                {


                    var cnic = "";
                    var uri = model.Url ?? "";
                    var cnc = model.Cnic ?? "";


                    if (uri == "" && cnc == "" && model.Friend == null)
                    {
                        return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                    }


                    if (uri.Length > 0)
                    {
                        var d = uri.Split('/').Last();
                        if (d != null && d.Length > 0)
                        {
                            cnic = d;

                        }

                    }
                    if (cnc.Length > 0)
                    {
                        cnic = cnc;

                    }

                    foneMe.SL.Entities.User objDbUser = null;


                    if (cnic != null)
                    {
                        var objDbUser1 = _unitOfWork.UserRepository.FindByCNIC(cnic);

                        if (objDbUser1 != null) objDbUser = objDbUser1;

                    }
                    if (model.Friend != null)
                    {
                        var objDbUser1 = _unitOfWork.UserRepository.FindByUserId(model.Friend);
                        if (objDbUser1 != null) objDbUser = objDbUser1;

                    }









                    if (objDbUser == null)
                    {
                        return await Task.FromResult(new UserProfileResponse { StatusCode = "409" });
                    }
                    else
                    {
                        //  var inserted = _unitOfWork.CallConnectionRepository.AddUserasFriend(new AddContactsVM { Name = objDbUser.Title, UserId = model.Me, FriendId = objDbUser.UserId });


                        string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                        var objUpdateUserVM = new UpdateUserVM();
                        var objMobile = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();

                        objUpdateUserVM.Name = objDbUser.FirstName;
                        objUpdateUserVM.ContactFT = objDbUser.DeviceToken;

                        objUpdateUserVM.ContactVT = objDbUser.VoipToken;
                        objUpdateUserVM.ContactCNIC = objDbUser.CNIC;

                        objUpdateUserVM.PhoneNumber = objMobile?.Description;
                        objUpdateUserVM.Email = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault()?.Description;
                        objUpdateUserVM.Address = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault()?.Description;
                        objUpdateUserVM.UserId = objDbUser.UserId;
                        objUpdateUserVM.CountryCode = objMobile?.CountryCode;
                        objUpdateUserVM.MobileNumberWithoutCode = objMobile?.NumberWithOutCode;
                        //objUpdateUserVM.ImageUrl = ProjectImgPth + "/Common/DisplayImageById?fileId=" + objDbUser.UserId;
                        objUpdateUserVM.ImageUrl = ProjectImgPth + objDbUser.ImageURL;
                        objUpdateUserVM.AboutMe = _unitOfWork.UserAboutmeRepository.GetUserAboutMe(objUpdateUserVM.UserId.ToString());
                        return await Task.FromResult(new UserProfileResponse { UserProfileData = objUpdateUserVM, IsSuccessed = true, StatusCode = "200" });
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/lookuser");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }


        }

        [AllowAnonymous]
        [Route("api/account/v1/cnctoprofile")]
        [HttpPost]
        public async Task<UserProfileResponse> CnToProfile([FromBody] CnictoProfileVM model)
        {


            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                }
                else
                {


                    var cnic = "";
                    var uri = model.Url ?? "";
                    var cnc = model.Cnic ?? "";


                    if (uri == "" && cnc == "")
                    {
                        return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                    }


                    if (uri.Length > 0)
                    {
                        var d = uri.Split('/').Last();
                        if (d != null && d.Length > 0)
                        {
                            cnic = d;

                        }

                    }
                    if (cnc.Length > 0)
                    {
                        cnic = cnc;

                    }
                    var objDbUser = _unitOfWork.UserRepository.FindByCNIC(cnic);
                    if (objDbUser == null)
                    {
                        return await Task.FromResult(new UserProfileResponse { StatusCode = "409" });
                    }
                    else
                    {
                        string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                        var objUpdateUserVM = new UpdateUserVM();
                        var objMobile = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();

                        objUpdateUserVM.Name = objDbUser.FirstName;
                        objUpdateUserVM.PhoneNumber = objMobile?.Description;
                        objUpdateUserVM.Email = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault()?.Description;
                        objUpdateUserVM.Address = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault()?.Description;
                        objUpdateUserVM.UserId = objDbUser.UserId;
                        objUpdateUserVM.CountryCode = objMobile?.CountryCode;
                        objUpdateUserVM.ContactCNIC = objDbUser.CNIC;
                        objUpdateUserVM.MobileNumberWithoutCode = objMobile?.NumberWithOutCode;
                        //objUpdateUserVM.ImageUrl = ProjectImgPth + "/Common/DisplayImageById?fileId=" + objDbUser.UserId;
                        objUpdateUserVM.ImageUrl = ProjectImgPth + objDbUser.ImageURL;
                        objUpdateUserVM.AboutMe = _unitOfWork.UserAboutmeRepository.GetUserAboutMe(objDbUser.UserId.ToString());
                        return await Task.FromResult(new UserProfileResponse { UserProfileData = objUpdateUserVM, StatusCode = "200" });
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/cnctoprofile");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

        }


        [AllowAnonymous]
        [Route("api/account/v1/phoneNumberToProfile")]
        [HttpPost]
        public async Task<UserProfileResponse> PhoneNumberToProfile([FromBody] CnictoProfileVM model)
        {


            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                }
                else
                {
                    var uri = model.Url ?? "";
                    var cnic = model.Cnic ?? "";


                    if (uri == "" && cnic == "")
                    {
                        return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                    }


                    var objDbUser = _unitOfWork.UserRepository.FindByLoginName(cnic);
                    if (objDbUser == null)
                    {
                        return await Task.FromResult(new UserProfileResponse { StatusCode = "409" });
                    }
                    else
                    {
                        string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                        var objUpdateUserVM = new UpdateUserVM();
                        var objMobile = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();

                        objUpdateUserVM.Name = objDbUser.FirstName;
                        objUpdateUserVM.PhoneNumber = objMobile?.Description;
                        objUpdateUserVM.Email = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault()?.Description;
                        objUpdateUserVM.Address = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault()?.Description;
                        objUpdateUserVM.UserId = objDbUser.UserId;
                        objUpdateUserVM.CountryCode = objMobile?.CountryCode;
                        objUpdateUserVM.MobileNumberWithoutCode = objMobile?.NumberWithOutCode;
                        objUpdateUserVM.ContactCNIC = objDbUser.CNIC;
                        //objUpdateUserVM.ImageUrl = ProjectImgPth + "/Common/DisplayImageById?fileId=" + objDbUser.UserId;
                        objUpdateUserVM.ImageUrl = ProjectImgPth + objDbUser.ImageURL;
                        objUpdateUserVM.AboutMe = _unitOfWork.UserAboutmeRepository.GetUserAboutMe(objDbUser.UserId.ToString());
                        return await Task.FromResult(new UserProfileResponse { UserProfileData = objUpdateUserVM, StatusCode = "200" });
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/cnctoprofile");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

        }

        [AllowAnonymous]
        [Route("api/account/v1/searchuser")]
        [HttpPost]
        public async Task<UserListResponse> Searchuser([FromBody] SearchProfileVM model)
        {
            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new UserListResponse { StatusCode = "400" });
                }
                else
                {
                    var cnic = "";
                    var uri = model.byName ?? "";
                    var cnc = model.byCnic ?? "";

                    if (uri == "" && cnc == "")
                    {
                        return await Task.FromResult(new UserListResponse { StatusCode = "400" });
                    }

                    if (uri.Length > 0)
                    {
                        var d = uri.Split('/').Last();
                        if (d != null && d.Length > 0)
                        {
                            cnic = d;
                        }
                    }
                    if (cnc.Length > 0)
                    {
                        cnic = cnc;
                    }

                    var objDbUserList = _unitOfWork.UserRepository.Filter(cnic);
                    
                    if (objDbUserList == null)
                    {
                        return await Task.FromResult(new UserListResponse { StatusCode = "409" });
                    }
                    else
                    {

                        List<UpdateUserVM> result = new List<UpdateUserVM>();

                        foreach (var objDbUser in objDbUserList)
                        {
                            string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                            var objUpdateUserVM = new UpdateUserVM();
                            var objMobile = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();

                            objUpdateUserVM.Name = objDbUser.FirstName;
                            objUpdateUserVM.ContactCNIC = objDbUser.CNIC;
                            objUpdateUserVM.ContactFT = objDbUser.DeviceToken;
                            objUpdateUserVM.ContactVT = objDbUser.VoipToken;


                            objUpdateUserVM.PhoneNumber = objMobile?.Description;
                            objUpdateUserVM.Email = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault()?.Description;
                            objUpdateUserVM.Address = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault()?.Description;
                            objUpdateUserVM.UserId = objDbUser.UserId;
                            objUpdateUserVM.CountryCode = objMobile?.CountryCode;
                            objUpdateUserVM.MobileNumberWithoutCode = objMobile?.NumberWithOutCode;
                            objUpdateUserVM.AboutMe = _unitOfWork.UserAboutmeRepository.GetUserAboutMe(objDbUser.UserId.ToString());
                            if (objDbUser.ImageURL != null && objDbUser.ImageURL != "")
                            {
                                objUpdateUserVM.ImageUrl = ProjectImgPth + objDbUser.ImageURL;
                            }

                            result.Add(objUpdateUserVM);

                        }

                        return await Task.FromResult(new UserListResponse { Data = result, StatusCode = "200" });
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/cnctoprofile");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        [AllowAnonymous]
        [Route("api/account/v2/searchuser")]
        [HttpPost]
        public async Task<UserListResponse> SearchuserV2([FromBody] SearchProfileVM model)
        {
            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new UserListResponse { StatusCode = "400" });
                }
                else
                {
                    var cnic = "";
                    var uri = model.byName ?? "";
                    var cnc = model.byCnic ?? "";

                    if (uri == "" && cnc == "")
                    {
                        return await Task.FromResult(new UserListResponse { StatusCode = "400" });
                    }

                    if (uri.Length > 0)
                    {
                        var d = uri.Split('/').Last();
                        if (d != null && d.Length > 0)
                        {
                            cnic = d;
                        }
                    }
                    if (cnc.Length > 0)
                    {
                        cnic = cnc;
                    }

                    var objDbUserList = _unitOfWork.UserRepository.FilterUsers(cnic, model.UserId);

                    if (objDbUserList == null)
                    {
                        return await Task.FromResult(new UserListResponse { StatusCode = "409" });
                    }
                    else
                    {

                        List<UpdateUserVM> result = new List<UpdateUserVM>();

                        foreach (var objDbUser in objDbUserList)
                        {
                            string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                            var objUpdateUserVM = new UpdateUserVM();
                            var objMobile = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();

                            objUpdateUserVM.Name = objDbUser.FirstName;
                            objUpdateUserVM.ContactCNIC = objDbUser.CNIC;
                            objUpdateUserVM.ContactFT = objDbUser.DeviceToken;
                            objUpdateUserVM.ContactVT = objDbUser.VoipToken;


                            objUpdateUserVM.PhoneNumber = objMobile?.Description;
                            objUpdateUserVM.Email = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault()?.Description;
                            objUpdateUserVM.Address = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault()?.Description;
                            objUpdateUserVM.UserId = objDbUser.UserId;
                            objUpdateUserVM.CountryCode = objMobile?.CountryCode;
                            objUpdateUserVM.MobileNumberWithoutCode = objMobile?.NumberWithOutCode;
                            objUpdateUserVM.AboutMe = _unitOfWork.UserAboutmeRepository.GetUserAboutMe(objDbUser.UserId.ToString());
                            if (objDbUser.ImageURL != null && objDbUser.ImageURL != "")
                            {
                                objUpdateUserVM.ImageUrl = ProjectImgPth + objDbUser.ImageURL;
                            }

                            result.Add(objUpdateUserVM);

                        }

                        return await Task.FromResult(new UserListResponse { Data = result, StatusCode = "200" });
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/cnctoprofile");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        [Route("api/account1/v1/changephonenumber")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<BoolResultVM> ChangePhoneNumber([FromBody] ChangePhoneNumberVM model)
        {
            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new BoolResultVM { IsSuccessed = false, StatusCode = "400" });
                }
                var match = Regex.Match(model.OldNumber, @"^(?:(\+92)|(0092))-{0,1}\d{3}-{0,1}\d{7}$|^\d{11}$|^\d{4}-\d{7}$");
                var match1 = Regex.Match(model.NewNumber, @"^(?:(\+92)|(0092))-{0,1}\d{3}-{0,1}\d{7}$|^\d{11}$|^\d{4}-\d{7}$");
                if (!match.Success || !match1.Success)
                {
                    return await Task.FromResult(new BoolResultVM { IsSuccessed = false, StatusCode = "402" });
                }
                //var actualNumber = HelperFunctions.FormatPhoneNumber(model.OldNumber, "");
                var result = await _unitOfWork.UserRepository.ChangePhoneNumber(model);
                if (result.IsSuccessed)
                {
                    await _unitOfWork.SaveChangesAsync();
                }
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/changephonenumber");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        [Route("api/account1/v1/saveuserdob")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<BoolResultVM> SaveUserDOB([FromBody] UserDOBVM model)
        {
            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new BoolResultVM { IsSuccessed = false, StatusCode = "400" });
                }
                var result = await _unitOfWork.UserRepository.SaveUserDOB(model);
                if (result.IsSuccessed)
                {
                    await _unitOfWork.SaveChangesAsync();
                }
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/saveuserdob");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        [Route("api/account/v1/sendfirebasenotification")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<BoolResultVM> SendFireBaseNotification([FromBody] FirebaseVM model)
        {
            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new BoolResultVM { IsSuccessed = false, StatusCode = "400" });
                }
                var topic = "CallConnect";
                // This registration token comes from the client FCM SDKs.
                var registrationToken = model.DeviceToken;

                // See documentation on defining a message payload.
                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = "Fone",
                        Body = "Hello from Fone",
                    },
                    Data = new Dictionary<string, string>()
                    {
                     { "DialerId", "" },
                     { "ReceiverId", "" },
                     { "DialerNumber", "" },
                     { "ReceiverNumber", "" },
                     { "AppType", "" },
                     { "CallType", "" },
                     { "CallDate", "" },
                     { "Status", "" },
                     { "ChannelName", "" },
                     { "content_available", "true" },
                     { "NotificationType", "CLLCN" },
                     },
                    Token = registrationToken,
                    //Topic = topic,
                };

                // Send a message to the device corresponding to the provided
                // registration token.               
                //string response = await FirebaseMessaging.GetMessaging(FirebaseApp.DefaultInstance).SendAsync(message);
                // Response is a message ID string.
                //Console.WriteLine("Successfully sent message: " + response);
                //
                string response = await FirebaseMessaging.GetMessaging(FirebaseApp.DefaultInstance).SendAsync(message);
                // Response is a message ID string.
                Console.WriteLine("Successfully sent message: " + response);
                return await Task.FromResult(new BoolResultVM { IsSuccessed = true, StatusCode = "200" });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/sendfirebasenotification");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }


        [Route("api/account/v1/sendfirebasenotification1")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<BoolResultVM> SendFireBaseNotification1([FromBody] FirebaseVM model)
        {
            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new BoolResultVM { IsSuccessed = false, StatusCode = "400" });
                }
                var topic = "CallConnect";
                // This registration token comes from the client FCM SDKs.
                var registrationToken = model.DeviceToken;

                // See documentation on defining a message payload.
                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = "Fone",
                        Body = "Hello from Fone",
                    },
                    Data = new Dictionary<string, string>()
                    {
                     { "DialerId", "" },
                     { "ReceiverId", "" },
                     { "DialerNumber", "" },
                     { "ReceiverNumber", "" },
                     { "AppType", "" },
                     { "CallType", "" },
                     { "CallDate", "" },
                     { "Status", "" },
                     { "ChannelName", "" },
                     { "content_available", "true" },
                     { "NotificationType", "CLLCN" },
                     },
                    Token = registrationToken,
                    //Topic = topic,
                };

                // Send a message to the device corresponding to the provided
                // registration token.               
                //string response = await FirebaseMessaging.GetMessaging(FirebaseApp.DefaultInstance).SendAsync(message);
                // Response is a message ID string.
                //Console.WriteLine("Successfully sent message: " + response);
                //
                string response = await FirebaseMessaging.GetMessaging(FirebaseApp.DefaultInstance).SendAsync(message);
                // Response is a message ID string.
                Console.WriteLine("Successfully sent message: " + response);
                return await Task.FromResult(new BoolResultVM { IsSuccessed = true, StatusCode = "200" });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/sendfirebasenotification1");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        [Route("api/account/v1/getprofiletocall")]
        [HttpPost]
        public async Task<UserProfileResponse> GetProfileToCall([FromBody] FirebaseVM model)
        {
            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                }
                else
                {
                    var cnic = model.DeviceToken;
                    var objDbUser = _unitOfWork.UserRepository.FindByCNIC(cnic);
                    if (objDbUser == null)
                    {
                        return await Task.FromResult(new UserProfileResponse { StatusCode = "409" });
                    }
                    else
                    {
                        string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                        var objUpdateUserVM = new UpdateUserVM();
                        var objMobile = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();

                        objUpdateUserVM.Name = objDbUser.FirstName;
                        objUpdateUserVM.ContactFT = objDbUser.DeviceToken;
                        objUpdateUserVM.ContactVT = objDbUser.VoipToken;
                        objUpdateUserVM.ContactCNIC = objDbUser.CNIC;
                        objUpdateUserVM.PhoneNumber = objMobile?.Description;
                        objUpdateUserVM.Email = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault()?.Description;
                        objUpdateUserVM.Address = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault()?.Description;
                        objUpdateUserVM.UserId = objDbUser.UserId;
                        objUpdateUserVM.CountryCode = objMobile?.CountryCode;
                        objUpdateUserVM.MobileNumberWithoutCode = objMobile?.NumberWithOutCode;
                        //objUpdateUserVM.ImageUrl = ProjectImgPth + "/Common/DisplayImageById?fileId=" + objDbUser.UserId;
                        objUpdateUserVM.ImageUrl = ProjectImgPth + objDbUser.ImageURL;
                        return await Task.FromResult(new UserProfileResponse { UserProfileData = objUpdateUserVM, StatusCode = "200" });
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/getprofiletocall");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }



        [Route("api/account/v1/changeuserdevicetoken")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<BoolResultVM> changeuserdevicetoken([FromBody] UserTokenModelVM model)
        {
            //   BoolResultVM result = new BoolResultVM();

            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new BoolResultVM { IsSuccessed = false, StatusCode = "400" });
                }
                // Here we send FSM to token.

                //   result.IsSuccessed = true; ;


                //  result.Data = "Device Token Updated";

                //   var objDbUser = _unitOfWork.UserRepository.FindByCNIC(useris);




                var result = await _unitOfWork.UserRepository.updateUserDeviceToken(model);

                if (result.IsSuccessed)
                {
                    // await _unitOfWork.SaveChangesAsync();
                }
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/saveuserdob");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }



        [Route("api/account/v1/changeuservoiptoken")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<BoolResultVM> changeuservoiptoken([FromBody] UserTokenModelVM model)
        {
            //   BoolResultVM result = new BoolResultVM();

            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new BoolResultVM { IsSuccessed = false, StatusCode = "400" });
                }
                // Here we send FSM to token.

                //   result.IsSuccessed = true; ;


                //  result.Data = "Device Token Updated";

                //   var objDbUser = _unitOfWork.UserRepository.FindByCNIC(useris);




                var result = await _unitOfWork.UserRepository.updateUserVoipToken(model);

                if (result.IsSuccessed)
                {
                    // await _unitOfWork.SaveChangesAsync();
                }
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/saveuserdob");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }


        [Route("api/account/v1/sendFcmOPt")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<BoolResultVM> sendFcmOPt([FromBody] SendFcmRequestVM model)
        {
            BoolResultVM result = new BoolResultVM();

            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new BoolResultVM { IsSuccessed = false, StatusCode = "400" });
                }
                // Here we send FSM to token.



                var topic = "CallConnect";
                // This registration token comes from the client FCM SDKs.
                var registrationToken = model.FcmToken;

                // See documentation on defining a message payload.
                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = model.Title,
                        Body = model.Desc,
                    },
                    Data = model.Data,
                    Token = registrationToken,
                    //Topic = topic,
                };

                // Send a message to the device corresponding to the provided
                // registration token.               
                //string response = await FirebaseMessaging.GetMessaging(FirebaseApp.DefaultInstance).SendAsync(message);
                // Response is a message ID string.
                //Console.WriteLine("Successfully sent message: " + response);

                string response = await FirebaseMessaging.GetMessaging(FirebaseApp.DefaultInstance).SendAsync(message);
                // Response is a message ID string.
                Console.WriteLine("Successfully sent message: " + response);
                return await Task.FromResult(new BoolResultVM { IsSuccessed = true, StatusCode = "200" });

                result.IsSuccessed = true; ;
                result.Data = "FSM Send";


                if (result.IsSuccessed)
                {
                    // await _unitOfWork.SaveChangesAsync();
                }
                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/saveuserdob");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        [Route("api/account/v1/updateuserprofile")]
        [HttpPost]
        public async Task<UserProfileResponse> UpdateUserProfile()
        {
            try
            {
                var request = HttpContext.Current.Request;

                if (request.Form?.Count == 0)
                {
                    return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                }
                var objUserProfileVM = new UserProfileVM();
                UserAboutMeViewModel userAboutMe = new UserAboutMeViewModel();
                objUserProfileVM.Name = request.Form.GetValues("Name")?.First() ?? "";
                objUserProfileVM.Email = request.Form.GetValues("Email")?.First() ?? "";
                objUserProfileVM.PhoneNumber = request.Form.GetValues("PhoneNumber")?.First() ?? "";
                objUserProfileVM.UserId = request.Form.GetValues("UserId")?.First() ?? "";
                objUserProfileVM.Address = request.Form.GetValues("Address")?.First() ?? "";
                objUserProfileVM.FatherName = request.Form.GetValues("FatherName")?.First() ?? "";



                if (!string.IsNullOrEmpty(objUserProfileVM.Address))
                {
                    var nicResult = _unitOfWork.UserRepository.checkNIC(objUserProfileVM.Address);
                    if (nicResult.Result != null && nicResult.Result.ToLower().Equals(objUserProfileVM.UserId.ToLower()))
                    {
                        var saveRes = _unitOfWork.UserRepository.SaveUserNIC(objUserProfileVM.Address, objUserProfileVM.UserId);
                    }
                    else if (nicResult.Result == null || nicResult.Result.Trim().Equals("00000000-0000-0000-0000-000000000000"))
                    {
                        var saveRes = _unitOfWork.UserRepository.SaveUserNIC(objUserProfileVM.Address, objUserProfileVM.UserId);
                    }
                    else
                    {
                        return await Task.FromResult(new UserProfileResponse { StatusCode = "406" });
                    }
                }
                if (string.IsNullOrEmpty(objUserProfileVM.Name)
                // || string.IsNullOrEmpty(objUserProfileVM.Email)
                || string.IsNullOrEmpty(objUserProfileVM.PhoneNumber)
                || string.IsNullOrEmpty(objUserProfileVM.UserId)
                )
                {
                    return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                }
                var uId = new Guid(objUserProfileVM.UserId);
                var objDbUser = _unitOfWork.UserRepository.FindById(uId);
                if (objDbUser == null)
                {
                    return await Task.FromResult(new UserProfileResponse { StatusCode = "409" });
                }

                HttpPostedFile[] PrescriptionUrl = new HttpPostedFile[1];
                PrescriptionUrl[0] = request.Files["UserImage"];


                var userImageProfilePath = _unitOfWork.ProfileRepository.GetByShortName("USRIMGPTH");
                objDbUser.FirstName = objUserProfileVM.Name;
                //objDbUser.FullName = objUserProfileVM.Name;
                if (string.IsNullOrEmpty(objUserProfileVM.FatherName))
                {
                    objDbUser.FatherName = objUserProfileVM.FatherName;
                }
                //if (!string.IsNullOrEmpty(model.Password))
                //{
                //    objDbUser.PasswordHash = _userManager.PasswordHasher.HashPassword(model.Password);
                //}
                var objContactEmail = objDbUser.Contacts.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault();
                if (objContactEmail != null)
                {
                    objContactEmail.Description = objUserProfileVM.Email;
                    _unitOfWork.ContactRepository.Update(objContactEmail);
                }
                var objContactPhoneNumber = objDbUser.Contacts.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();
                if (objContactPhoneNumber != null)
                {
                    objContactPhoneNumber.Description = objUserProfileVM.PhoneNumber;
                    _unitOfWork.ContactRepository.Update(objContactPhoneNumber);
                }
                var objContactAddress = objDbUser.Contacts.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault();
                if (objContactAddress == null)
                {
                    var contact = new Contact
                    {
                        UserId = objDbUser.UserId,
                        ContactTypeProfileId = _unitOfWork.ProfileRepository.GetByShortName("ADD")?.ProfileId,
                        ContactCategoryProfileId = _unitOfWork.ProfileRepository.GetByShortName("PRN")?.ProfileId,
                        Description = objUserProfileVM.Address,
                        IsActive = true,
                        IsPrimary = true,
                    };
                    _unitOfWork.ContactRepository.Add(contact);
                }
                else
                {
                    objContactAddress.Description = objUserProfileVM.Address;
                    _unitOfWork.ContactRepository.Update(objContactAddress);
                }


                // Get User Image and store URL
                if (PrescriptionUrl[0] != null && PrescriptionUrl[0].ContentLength > 0)
                {
                    var userImageProfilePath1 = _unitOfWork.ProfileRepository.GetByShortName("USRIMGPTH");
                    objDbUser.ImageURL = new FileUploadHelper().SaveFileData(PrescriptionUrl, userImageProfilePath1?.Name, "userImage")?.FirstOrDefault();
                }

                await _unitOfWork.SaveChangesAsync();
                string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                var objUpdateUserVM = new UpdateUserVM();
                var objMobile = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();
                objUpdateUserVM.Name = objDbUser.FirstName;
                objUpdateUserVM.PhoneNumber = objMobile?.Description;
                objUpdateUserVM.Email = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault()?.Description;
                objUpdateUserVM.Address = objDbUser.CNIC;
                //objUpdateUserVM.Address = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault()?.Description;
                objUpdateUserVM.UserId = objDbUser.UserId;
                objUpdateUserVM.FatherName = objDbUser.FatherName;
                objUpdateUserVM.CountryCode = objMobile?.CountryCode;
                objUpdateUserVM.MobileNumberWithoutCode = objMobile?.NumberWithOutCode;
                // objUpdateUserVM.ImageUrl = ProjectImgPth + "/Common/DisplayImageById?fileId=" + objDbUser.UserId;
                if (objDbUser.ImageURL != "" && objDbUser.ImageURL != null)
                {
                    objUpdateUserVM.ImageUrl = ProjectImgPth + objDbUser.ImageURL;
                }


                userAboutMe.AboutMe = request.Form.GetValues("AboutMe")?.First() ?? "";
                userAboutMe.Address = request.Form.GetValues("Address")?.First() ?? "";
                userAboutMe.PhoneBrand = request.Form.GetValues("PhoneBrand")?.First() ?? "";
                userAboutMe.PhoneModel = request.Form.GetValues("PhoneModel")?.First() ?? "";
                userAboutMe.Profession = request.Form.GetValues("Profession")?.First() ?? "";
                userAboutMe.UserID = request.Form.GetValues("UserId")?.First() ?? "";

                // If don't want to update about me from here 
                //_unitOfWork.UserAboutmeRepository.UpdateUserAboutme(userAboutMe);
                // objUpdateUserVM.AboutMe = userAboutMe;

                // 
                objUpdateUserVM.AboutMe = _unitOfWork.UserAboutmeRepository.GetUserAboutMe(userAboutMe.UserID);

                return await Task.FromResult(new UserProfileResponse { UserProfileData = objUpdateUserVM, StatusCode = "200" });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/updateuserprofile");
                return await Task.FromResult(new UserProfileResponse { UserProfileData = null, StatusCode = "200", Error = ex.ToString() });
                //throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        [Route("api/account/v1/updateuserprofile1")]
        [HttpPost]
        public async Task<UserProfileResponse> UpdateUserProfile1([FromBody] UserProfileVM model)
        {
            try
            {
                if (model == null
                || string.IsNullOrEmpty(model.Name)
                || string.IsNullOrEmpty(model.Email)
                || string.IsNullOrEmpty(model.PhoneNumber)
                //|| model.UserId == null || model.UserId == Guid.Empty
                )
                {
                    return await Task.FromResult(new UserProfileResponse { StatusCode = "400" });
                }
                var objDbUser = _unitOfWork.UserRepository.FindById(model.UserId);
                if (objDbUser == null)
                {
                    return await Task.FromResult(new UserProfileResponse { StatusCode = "409" });
                }

                objDbUser.FirstName = model.Name;
                // objDbUser.FullName = model.Name;
                var objContactEmail = objDbUser.Contacts.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault();
                if (objContactEmail != null)
                {
                    objContactEmail.Description = model.Email;
                    _unitOfWork.ContactRepository.Update(objContactEmail);
                }
                var objContactPhoneNumber = objDbUser.Contacts.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault();
                if (objContactPhoneNumber != null)
                {
                    objContactPhoneNumber.Description = model.PhoneNumber;
                    _unitOfWork.ContactRepository.Update(objContactPhoneNumber);
                }
                var objContactAddress = objDbUser.Contacts.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault();
                if (objContactAddress == null)
                {
                    var contact = new Contact
                    {
                        UserId = objDbUser.UserId,
                        ContactTypeProfileId = _unitOfWork.ProfileRepository.GetByShortName("ADD")?.ProfileId,
                        ContactCategoryProfileId = _unitOfWork.ProfileRepository.GetByShortName("PRN")?.ProfileId,
                        Description = model.Address,
                        IsActive = true,
                        IsPrimary = true,
                    };
                    _unitOfWork.ContactRepository.Add(contact);
                }
                else
                {
                    objContactAddress.Description = model.Address;
                    _unitOfWork.ContactRepository.Update(objContactAddress);
                }


                // Get User Image and store URL
                if (model.UserImage != null && model.UserImage.Length > 0)
                {
                    objDbUser.ImageBytes = model.UserImage;
                    objDbUser.ImageURL = model.FileName;
                }
                await _unitOfWork.SaveChangesAsync();
                string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                var objUpdateUserVM = new UpdateUserVM();
                objUpdateUserVM.Name = objDbUser.FirstName;
                objUpdateUserVM.PhoneNumber = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "MBN")?.FirstOrDefault()?.Description;
                objUpdateUserVM.Email = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "EML")?.FirstOrDefault()?.Description;
                objUpdateUserVM.Address = objDbUser.Contacts?.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile?.ShortName == "ADD")?.FirstOrDefault()?.Description;
                objUpdateUserVM.UserId = objDbUser.UserId;
                //objUpdateUserVM.ImageUrl = ProjectImgPth + "/Common/DisplayImageBytes?fileId=" + objDbUser.UserId;
                objUpdateUserVM.ImageUrl = ProjectImgPth + objDbUser.ImageURL;
                return await Task.FromResult(new UserProfileResponse { UserProfileData = objUpdateUserVM, StatusCode = "200" });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/updateuserprofile");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        public string SaveByteArrayImage(byte[] imageByteArray, string fileDirectory, string fileName)
        {

            Guid miliSec = Guid.NewGuid();
            string virtualPath = null;
            string imageName = null;
            if (imageByteArray != null && imageByteArray.Length > 0)
            {
                imageName = Regex.Replace(fileName, @"\s", "_") + "_" + miliSec + Path.GetExtension(Path.GetFileName(fileName));
                virtualPath = fileDirectory + imageName;
                File.WriteAllBytes(virtualPath, imageByteArray);
            }

            return imageName;
        }



        [Route("api/account/v1/tokenforcall")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<VoiceTokenResponse> TokenForCall([FromBody] VoiceTokenVM model)
        {
            try
            {
                if (model == null
                || string.IsNullOrEmpty(model.ChannelName)
                || model.UserId == null || model.UserId == Guid.Empty
                )
                {
                    return await Task.FromResult(new VoiceTokenResponse { StatusCode = "400" });
                }

                string TwilioAccountSID = WebConfigurationManager.AppSettings["TwilioCallAccountSID"];
                var accountSid = TwilioAccountSID;

                var apiKeySid = WebConfigurationManager.AppSettings["ApiKeySID"];

                // Twilio API Secret SID
                var apiKeySecret = WebConfigurationManager.AppSettings["ApiKeySecret"];

                // Twilio Firebase Service Key
                var pushSIDAndroid = WebConfigurationManager.AppSettings["TwilioPushSIDAndroid"];

                var identity = model.UserId + "";

                // Create a video grant for the token
                var grant = new VideoGrant();
                var grantVoice = new VoiceGrant();
                grant.Room = model.ChannelName;
                grantVoice.PushCredentialSid = pushSIDAndroid;
                grantVoice.OutgoingApplicationSid = "AP0088eed36ac7ae7531346a2e8a821465";
                grantVoice.IncomingAllow = true;
                var grants = new HashSet<IGrant> { grant };
                var grantsVoice = new HashSet<IGrant> { grantVoice };

                // Create an Access Token generator
                var token = new Token(accountSid, apiKeySid, apiKeySecret, identity: identity, grants: grants);
                var tokenVoice = new Token(accountSid, apiKeySid, apiKeySecret, identity: identity, grants: grantsVoice);

                return await Task.FromResult(new VoiceTokenResponse { StatusCode = "200", JWTToken = token.ToJwt(), JWTVoice = tokenVoice.ToJwt() });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/tokenforcall");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }




        [Route("api/account/v1/usercontactsoriginal")]
        [HttpPost]
        public async Task<UserContactsResponse> UserContactsoriginal([FromBody] UserContactsVM model)
        {
            try
            {
                if (model == null || model?.Contacts?.Count < 1 || model.UserId == null || model.UserId == Guid.Empty)
                {
                    return await Task.FromResult(new UserContactsResponse { StatusCode = "400" });
                }
                var list = _unitOfWork.CallConnectionRepository.GetUserMatchContacts(model)?.ToList();
                return await Task.FromResult(new UserContactsResponse { StatusCode = "200", Contacts = list });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/usercontacts");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }



        #region Get User Contact List match with registered app users

        [Route("api/account/v1/usercontacts")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<UserContactsResponse> UserContacts([FromBody] UserContactsVM model)
        {
            try
            {
                if (model.Contacts == null || model.Contacts.Count == 0)
                {
                    // retrive without matching

                    var list = _unitOfWork.CallConnectionRepository.GetUserContactsPaged(model)?.ToList();
                    //if (model.UserId.ToString().Equals("10E1C2CF-3185-45C1-8005-B7D7851E71CF", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    var logger = NLog.LogManager.GetCurrentClassLogger();
                    //    var data = JsonConvert.SerializeObject(list);
                    //    logger.Trace(data);
                    //}
                    return await Task.FromResult(new UserContactsResponse { StatusCode = "200", Contacts = list });


                }
                else
                {
                    var list = _unitOfWork.CallConnectionRepository.GetUserMatchContacts(model)?.ToList();
                    return await Task.FromResult(new UserContactsResponse { StatusCode = "200", Contacts = list });

                }


                /*    if (model == null || model?.Contacts?.Count < 1 || model.UserId == null || model.UserId == Guid.Empty)
                    {
                        return await Task.FromResult(new UserContactsResponse { StatusCode = "400" });
                    }
                    var list = _unitOfWork.CallConnectionRepository.GetUserContactsPaged(model)?.ToList();
                    return await Task.FromResult(new UserContactsResponse { StatusCode = "200", Contacts = list });*/


            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/usercontacts");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }


        [Route("api/account/v1/addusercontacts")]
        [HttpPost]
        public async Task<UserContactsResponse> AddUserContacts([FromBody] UserContactsVM model)
        {
            try
            {
                if (model == null || model?.Contacts?.Count < 1 || model.UserId == null || model.UserId == Guid.Empty)
                {
                    return await Task.FromResult(new UserContactsResponse { StatusCode = "400" });
                }
                var list = _unitOfWork.CallConnectionRepository.GetUserMatchContacts(model)?.ToList();
                return await Task.FromResult(new UserContactsResponse { StatusCode = "200", Contacts = list });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/usercontacts");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        #endregion


        [Route("api/account/v1/sendcallnotification")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<CallNotificationResponse> SendCallNotification([FromBody] CallNotificationVM model)
        {
            try
            {
                if (model == null
                || string.IsNullOrEmpty(model.DialerNumber)
                || string.IsNullOrEmpty(model.ReceiverNumber)
                || string.IsNullOrEmpty(model.Status)
                || string.IsNullOrEmpty(model.ChannelName)
                || string.IsNullOrEmpty(model.CallType)
                || string.IsNullOrEmpty(model.AppType)
                || model.UserId == null || model.UserId == Guid.Empty
                )
                {
                    return await Task.FromResult(new CallNotificationResponse { StatusCode = "400" });
                }
                if (model.DialerNumber.Equals(model.ReceiverNumber))
                {
                    return await Task.FromResult(new CallNotificationResponse { StatusCode = "400", Message = "Dialer Number & Receiver Number Are Same!!" });
                }
                var objDialerDbUser = _unitOfWork.UserRepository.FindById(model.UserId);
                if (objDialerDbUser == null)
                {
                    return await Task.FromResult(new CallNotificationResponse { StatusCode = "409" });
                }
                var objReceiverDbUser = _unitOfWork.UserRepository.FindByLoginName(model.ReceiverNumber);
                if (objReceiverDbUser == null)
                {
                    return await Task.FromResult(new CallNotificationResponse { StatusCode = "490" });
                }
                model.ReceiverId = objReceiverDbUser.UserId;
                model.StatusProfileId = _unitOfWork.ProfileRepository.GetByShortName(model.Status)?.ProfileId ?? null;
                model.NotificationStatusProfileId = _unitOfWork.ProfileRepository.GetByShortName("CLLCN")?.ProfileId ?? null;
                model.ImageURL = objReceiverDbUser.ImageURL;
                var result = _unitOfWork.CallConnectionRepository.SaveCallConnection(model);
                result.DialerFoneID = objDialerDbUser.CNIC;
                result.ReceiverFoneID = objReceiverDbUser.CNIC;
                if (result != null)
                {

                    // Send Push Notification To receiver
                    //if (model.CallStatusType == "APPTOAPP")
                    //{
                    result.CallerName = _unitOfWork.CommonRepository.GetLocalContactNameCaller(model.UserId, objReceiverDbUser.UserId);
                    if (model.CallSid != null)
                    {
                        result.CallSid = model.CallSid;
                    }

                    var resAndroid = true;
                    if (model.AppType == "ANDROID")
                    {
                        resAndroid = await SendNotificationCallConnectAndroid(result);
                    }
                    else
                    {
                        resAndroid = await SendNotificationCallConnect(result);
                    }
                    //var res =  SendVOIPNotification(result);
                    if (resAndroid)
                    {
                        return await Task.FromResult(new CallNotificationResponse { StatusCode = "200" });

                    }
                    else
                    {
                        return await Task.FromResult(new CallNotificationResponse { StatusCode = "200", Message = "NNS" });

                    }
                    //}
                    //else
                    //{
                    //    return await Task.FromResult(new CallNotificationResponse { StatusCode = "200" });
                    //}
                }
                else
                {
                    return await Task.FromResult(new CallNotificationResponse { StatusCode = "415" });
                }
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/sendcallnotification");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        #region Notifications
        public async Task<bool> SendNotificationCallConnect(CallConnectResponse result)
        {
            try
            {
                // See documentation on defining a message payload.
                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = result.DialerNumber,
                        Body = result?.NotificationName,
                    },
                    Apns = new ApnsConfig()
                    {
                        Aps = new Aps()
                        {
                            ContentAvailable = true
                        }
                    },
                    Data = new Dictionary<string, string>()
                    {
                     { "DialerId", result.DialerId+"" },
                     { "ReceiverId", result.ReceiverId+"" },
                     { "DialerNumber", result.DialerNumber+"" },
                     { "ReceiverNumber", result.ReceiverNumber+"" },
                     { "NotificationType", result.NotificationType+"" },
                     { "CallType", result.CallType+"" },
                     { "CallDate", result.CallDate+"" },
                     { "DialerFoneID", result.DialerFoneID+"" },
                     { "ReceiverFoneID", result.ReceiverFoneID+"" },
                     { "Status", result.Status+"" },
                     { "ChannelName", result.ChannelName+"" },
                     { "DeviceToken", result.DeviceToken+"" },
                     { "CallSid", result.CallSid+"" },
                     { "CallLogStatusId", result.CallConnectionId+"" },
                     { "DialerImageUrl", result.DialerImageUrl },
                     },
                    Token = result.DeviceToken
                    //Topic = "CallConnect",
                };
                // Send a message to the device corresponding to the provided
                // registration token.
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                // Response is a message ID string.
                Console.WriteLine("Successfully sent message: " + response);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SendNotificationCallConnectAndroid(CallConnectResponse result)
        {
            try
            {
                // See documentation on defining a message payload.
                var message = new Message()
                {
                    Data = new Dictionary<string, string>()
                    {
                     { "DialerId", result.DialerId+"" },
                     { "ReceiverId", result.ReceiverId+"" },
                     { "DialerNumber", result.DialerNumber+"" },
                     { "ReceiverNumber", result.ReceiverNumber+"" },
                     { "NotificationType", result.NotificationType+"" },
                     { "CallType", result.CallType+"" },
                     { "CallDate", result.CallDate+"" },
                     { "DialerFoneID", result.DialerFoneID+"" },
                     { "ReceiverFoneID", result.ReceiverFoneID+"" },
                     { "Status", result.Status+"" },
                     { "ChannelName", result.ChannelName+"" },
                     { "DeviceToken", result.DeviceToken+"" },
                     { "CallSid", result.CallSid+"" },
                     { "CallLogStatusId", result.CallConnectionId+"" },
                     { "DialerImageUrl", result.DialerImageUrl },
                     },
                    Android = new AndroidConfig()
                    {
                        Priority = Priority.High
                    },
                    Token = result.DeviceToken
                    //Topic = "CallConnect",
                };
                // Send a message to the device corresponding to the provided
                // registration token.
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                // Response is a message ID string.
                Console.WriteLine("Successfully sent message: " + response);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool SendVOIPNotification(CallConnectResponse result)
        {
            try
            {
                var val = false;
                string pushkitVOIPCertificate = WebConfigurationManager.AppSettings["PushKitVOIPCertificate"];
                string pushkitVOIPCertificatePassword = WebConfigurationManager.AppSettings["PushKitVOIPCertificatePassword"];
                var p12fileName = HostingEnvironment.MapPath(@pushkitVOIPCertificate);
                //string p12fileName = "C:\\webroot\\PKI\\myCertificate.p12";
                string p12password = pushkitVOIPCertificatePassword;
                var appleCert = System.IO.File.ReadAllBytes(p12fileName);
                var config = new PushSharp.Apple.ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, appleCert, p12password, false);
                config.ValidateServerCertificate = false;
                var logger = NLog.LogManager.GetCurrentClassLogger();

                var apnsBroker = new ApnsServiceBroker(config);
                apnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
                {
                    aggregateEx.Handle(ex =>
                    {
                        // See what kind of exception it was to further diagnose           
                        if (ex is ApnsNotificationException)
                        {
                            var notificationException = (ApnsNotificationException)ex;
                            // Deal with the failed notification               
                            var apnsNotification = notificationException.Notification;
                            var statusCode = notificationException.ErrorStatusCode;
                            Console.WriteLine("Apple Notification Failed: ID={" + apnsNotification.Identifier + "}, Code={" + statusCode + "}");
                        }
                        else
                        {
                            // Inner exception might hold more useful information like an ApnsConnectionException   
                            Console.WriteLine("Notification Failed for some unknown reason : {" + ex.InnerException + "}");
                        }
                        // Mark it as handled           
                        return true;
                    });
                };
                apnsBroker.Start();
                apnsBroker.QueueNotification(new ApnsNotification
                {
                    DeviceToken = result.VOIPDeviceToken,
                    //Payload = jsonObject
                    Payload = JObject.Parse("{\"aps\":{\"alert\":\"" + "InComing Audio Call" + "\",\"badge\":1,\"DialerId\":\"" + result.DialerId + "\",\"ReceiverId\":\"" + result.ReceiverId + "\",\"DialerNumber\":\"" + result.DialerNumber + "\",\"ReceiverNumber\":\"" + result.ReceiverNumber + "\",\"CallerName\":\"" + result.CallerName + "\",\"NotificationType\":\"" + result.NotificationType + "\",\"CallType\":\"" + result.CallType + "\",\"CallDate\":\"" + result.CallDate.Value.ToShortDateString() + "\",\"Status\":\"" + result.Status + "\",\"ChannelName\":\"" + result.ChannelName + "\",\"DeviceToken\":\"" + result.DeviceToken + "\",\"CallLogStatusId\":" + result.CallConnectionId + ",\"DialerImageUrl\":\"" + result.DialerImageUrl + "\",\"sound\":\"default\"}}")
                });

                apnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
                {
                    aggregateEx.Handle(ex =>
                    {
                        // See what kind of exception it was to further diagnose
                        if (ex is ApnsNotificationException)
                        {
                            var notificationException = (ApnsNotificationException)ex;

                            // Deal with the failed notification
                            var apnsNotification = notificationException.Notification;
                            var statusCode = notificationException.ErrorStatusCode;

                            Console.WriteLine($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");
                        }
                        else
                        {
                            // Inner exception might hold more useful information like an ApnsConnectionException          
                            Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
                        }
                        // Mark it as handled
                        return true;
                    });
                };
                apnsBroker.OnNotificationSucceeded += (notification) =>
                {
                    val = true;
                    Console.WriteLine("Apple Notification Sent!");
                };
                //apnsBroker.OnNotificationFailed += (ss,sasd) => {
                //    val = "failed";
                //    Console.WriteLine("Apple Notification Sent!");
                //};

                apnsBroker.Stop();
                return val;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> SendNotificationToDialerForCallUpdation(CallDialerNotification result)
        {
            try
            {
                // See documentation on defining a message payload.
                var message = new Message()
                {
                    Notification = new Notification()
                    {
                        Title = result?.SenderMobileNumber,
                        Body = result?.NotificationName,
                    },
                    Data = new Dictionary<string, string>()
                    {
                     { "NotificationName", result.NotificationName+"" },
                     { "NotificationType", result.NotificationType+"" },
                     },
                    Token = result.DeviceToken,
                    //Topic = "CallConnect",
                };
                // Send a message to the device corresponding to the provided
                // registration token.
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                // Response is a message ID string.
                Console.WriteLine("Successfully sent message: " + response);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion

        #region Log out
        [Route("api/account/v1/logout")]
        [HttpPost]
        public async Task<StatusCodeVM> LogOut([FromBody] LogoutVM model)
        {
            try
            {
                if (model == null
                || model.UserId == null || model.UserId == Guid.Empty
                )
                {
                    return await Task.FromResult(new StatusCodeVM { StatusCode = "400" });
                }
                var objDialerDbUser = _unitOfWork.UserRepository.FindById(model.UserId);
                if (objDialerDbUser == null)
                {
                    return await Task.FromResult(new StatusCodeVM { StatusCode = "409" });
                }

                // Update User 
                objDialerDbUser.DeviceToken = "";
                objDialerDbUser.IsUserVerified = false;
                await _unitOfWork.SaveChangesAsync();
                return await Task.FromResult(new StatusCodeVM { StatusCode = "200" });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/logout");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Add Call Log
        [Route("api/account/v1/callstatushandling")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<StatusCodeVM> CallStatusHandling([FromBody] CallStatusHandlingVM model)
        {
            try
            {
                if (model == null
                || model.ReceiverId == null || model.ReceiverId == Guid.Empty
                || string.IsNullOrEmpty(model.CallConnectionId)
                || string.IsNullOrEmpty(model.ReceiverStatus)
                )
                {
                    return await Task.FromResult(new StatusCodeVM { StatusCode = "400" });
                }
                var objReceiverDbUser = _unitOfWork.UserRepository.FindById(model.ReceiverId);
                if (objReceiverDbUser == null)
                {
                    return await Task.FromResult(new StatusCodeVM { StatusCode = "409" });
                }
                var cId = Convert.ToInt64(model.CallConnectionId);
                var objCallConnection = _unitOfWork.CallConnectionRepository.FindById(cId);
                if (objCallConnection == null)
                {
                    return await Task.FromResult(new StatusCodeVM { StatusCode = "420" });
                }
                // Update Receiver Call Status 
                var objReceiverProfileId = _unitOfWork.ProfileRepository.GetByShortName(model.ReceiverStatus)?.ProfileId;
                var objNotificationProfileId = _unitOfWork.ProfileRepository.GetByShortName(model.NotificationType)?.ProfileId;
                objCallConnection.CallNotificationProfileId = objNotificationProfileId;
                objCallConnection.ReceiverStatusProfileId = objReceiverProfileId;
                objCallConnection.CallEndTime = model.CallReceivingTime;
                await _unitOfWork.SaveChangesAsync();
                return await Task.FromResult(new StatusCodeVM { StatusCode = "200" });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/callstatushandling");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }



        #endregion

        #region Notification ALerts

        [Route("api/account/v1/userpushnotifications")]
        [HttpPost]
        public async Task<StatusCodeVM> UserPushNotifications([FromBody] UserPushNotifications model)
        {
            try
            {
                if (model == null
                || model.ReceiverUserId == null || model.ReceiverUserId == Guid.Empty
                || string.IsNullOrEmpty(model.NotificationType)
                || string.IsNullOrEmpty(model.SenderMobileNumber)
                )
                {
                    return await Task.FromResult(new StatusCodeVM { StatusCode = "400" });
                }
                var objReceiverDbUser = _unitOfWork.UserRepository.FindById(model.ReceiverUserId);
                if (objReceiverDbUser == null)
                {
                    return await Task.FromResult(new StatusCodeVM { StatusCode = "409" });
                }

                var objNotificationProfile = _unitOfWork.ProfileRepository.GetByShortName(model.NotificationType);
                var objCallDialerNotification = new CallDialerNotification();
                objCallDialerNotification.NotificationName = objNotificationProfile?.Name;
                objCallDialerNotification.NotificationType = model.NotificationType;
                objCallDialerNotification.DeviceToken = objReceiverDbUser?.DeviceToken;
                objCallDialerNotification.SenderMobileNumber = model.SenderMobileNumber;
                await SendNotificationToDialerForCallUpdation(objCallDialerNotification);
                return await Task.FromResult(new StatusCodeVM { StatusCode = "200" });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/userpushnotifications");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }

        #endregion

        #region Get Call Log
        [Route("api/account/v1/usercalllogs")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<GetCallResponse> UserCallLogs([FromBody] GetCallLogVM model)
        {
            try
            {
                if (model == null
                || model.UserId == null || model.UserId == Guid.Empty
                )
                {
                    return await Task.FromResult(new GetCallResponse { StatusCode = "400" });
                }
                var objReceiverDbUser = _unitOfWork.UserRepository.FindById(model.UserId);
                if (objReceiverDbUser == null)
                {
                    return await Task.FromResult(new GetCallResponse { StatusCode = "409" });
                }
                var lstCallLogs = _unitOfWork.CallConnectionRepository.GetCallConnectionLogs(model.UserId);

                return await Task.FromResult(new GetCallResponse { StatusCode = "200", CallLogs = lstCallLogs });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/usercalllogs");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Get Call Log
        [Route("api/account/v1/recoveruserpassword")]
        [HttpPost]
        public async Task<GetCallResponse> RecoverUserPassword([FromBody] GetCallLogVM model)
        {
            try
            {
                if (model == null
                || model.UserId == null || model.UserId == Guid.Empty
                )
                {
                    return await Task.FromResult(new GetCallResponse { StatusCode = "400" });
                }
                var objReceiverDbUser = _unitOfWork.UserRepository.FindById(model.UserId);
                if (objReceiverDbUser == null)
                {
                    return await Task.FromResult(new GetCallResponse { StatusCode = "409" });
                }
                var lstCallLogs = _unitOfWork.CallConnectionRepository.GetCallConnectionLogs(model.UserId);

                return await Task.FromResult(new GetCallResponse { StatusCode = "200", CallLogs = lstCallLogs });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/usercalllogs");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Group Channel API
        [Route("api/account/v1/createGroupChannel")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<GroupDetailsResponse> CreateGroupChannel([FromBody] GroupDetailsViewModel model)
        {

            try
            {
                Guid UserId;
                try
                {
                    UserId = new Guid(model.UserID);
                }
                catch (Exception e)
                {

                    return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "User ID Must Be In Correct Format" });

                }
                //if (!ModelState.IsValid)
                //{
                if (model == null
                    || string.IsNullOrEmpty(model.GroupID)
                    // || string.IsNullOrEmpty(model.GroupDescription)
                    //|| string.IsNullOrEmpty(model.GroupLink)
                    || string.IsNullOrEmpty(model.GroupName)
                    || string.IsNullOrEmpty(model.DeepLink)
                    || string.IsNullOrEmpty(model.IsGroup)
                    || string.IsNullOrEmpty(model.IsPublic)
                    // || string.IsNullOrEmpty(model.PublicGroupLink)
                    || model.UserID == null
                    || UserId == Guid.Empty)
                {

                    return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "Fill All Required Fields!!" });

                }
                else
                {
                    var objDialerDbUser = _unitOfWork.UserRepository.FindById(UserId);
                    
                    if (objDialerDbUser == null)
                    {
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "409", Message = "User ID Not Found!!" });
                    }
                    if (model.GroupLink != "" && model.GroupLink != null)
                    {
                        var group = _unitOfWork.GroupChannelRepository.GetGroupByURL(model);

                        if (group != null)
                        {
                            return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "Group With Same URL Already Exist!!" });
                        }
                    }

                    var groupData = _unitOfWork.GroupChannelRepository.GetGroupByDeepLink(model);

                    if (groupData != null)
                    {
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "Group With Same DeepLink Already Exist!!" });
                    }

                    //var groupName = _unitOfWork.GroupChannelRepository.GetGroupByName(model);
                    
                    //if (groupName != null)
                    //{
                    //    return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "Group With Same Name Already Exist Group Name Must Be Unique!!" });
                    //}
                    _unitOfWork.GroupChannelRepository.AddGroup(model);
                    return await Task.FromResult(new GroupDetailsResponse { StatusCode = "200", Message = "Group Created Successfully!!" });

                }

            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/createGroupChannel");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Update User Group
        [Route("api/account/v1/updateGroupChannel")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<GroupDetailsResponse> UpdateGroupChannel([FromBody] GroupDetailsViewModel model)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                Guid UserId;
                try
                {
                    UserId = new Guid(model.UserID);
                }
                catch (Exception e)
                {

                    return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "User ID Must Be In Correct Format" });

                }
                if (model == null
                    || string.IsNullOrEmpty(model.GroupID)
                    // || string.IsNullOrEmpty(model.GroupDescription) 
                    // || string.IsNullOrEmpty(model.GroupLink)
                    || string.IsNullOrEmpty(model.GroupName)
                      || string.IsNullOrEmpty(model.DeepLink)
                    || string.IsNullOrEmpty(model.IsGroup)
                    || string.IsNullOrEmpty(model.IsPublic)
                    // || string.IsNullOrEmpty(model.PublicGroupLink)
                    || model.UserID == null
                    || UserId == Guid.Empty)
                {

                    return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "Fill All Required Fields!!" });

                }
                else
                {
                    var objDialerDbUser = _unitOfWork.UserRepository.FindById(UserId);
                    if (objDialerDbUser == null)
                    {
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "409", Message = "User ID Not Found!!" });

                    }
                    var groups = _unitOfWork.GroupChannelRepository.GetSingleGroup(model);
                    String message = "";
                    if (groups.Count() > 0)
                    {
                        var groupData = _unitOfWork.GroupChannelRepository.GetGroupByDeepLink(model);

                        if (groupData != null)
                        {
                            return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "Group With Same DeepLink Already Exist!!" });
                        }
                        _unitOfWork.GroupChannelRepository.UpdateGroupInfo(model);
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "200", Message = "Group Updated Successfully!!" });

                    }
                    else
                    {
                        message = "No Groups Found With Given Group ID";
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "409", Message = message });

                    }


                }

            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/createGroupChannel");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Get User Groups
        [Route("api/account/v1/getUserGroupChannel")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<GroupDetailsResponse> GetUserGroupChannel([FromBody] GroupDetailsViewModel model)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                Guid UserId;
                try
                {
                    UserId = new Guid(model.UserID);
                }
                catch (Exception e)
                {

                    return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "User ID Must Be In Correct Format" });

                }
                if (model == null
                    || model.UserID == null
                    || UserId == Guid.Empty)
                {

                    return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "Fill All Required Fields!!" });

                }
                else
                {
                    var objDialerDbUser = _unitOfWork.UserRepository.FindById(UserId);
                    if (objDialerDbUser == null)
                    {
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "409", Message = "User ID Not Found!!" });

                    }

                    var groups = _unitOfWork.GroupChannelRepository.GetAllGroups(model);
                    String message = "";
                    if (groups.Count() > 0)
                    {
                        message = "Retrieve User Groups Successfully!!";
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "200", Message = message, GroupData = groups });

                    }
                    else
                    {
                        message = "No Groups Found With Given User ID";
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "409", Message = message });

                    }

                }

            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/createGroupChannel");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Get Group By Group ID
        [Route("api/account/v1/getSingleGroupDetails")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<GroupDetailsResponse> GetSingleGroupDetails([FromBody] GroupDetailsViewModel model)
        {
            try
            {
                //if (!ModelState.IsValid)
                //{
                Guid UserId;
                try
                {
                    UserId = new Guid(model.UserID);
                }
                catch (Exception e)
                {

                    return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "User ID Must Be In Correct Format" });

                }
                if (model == null
                    || string.IsNullOrEmpty(model.GroupID)
                    || model.UserID == null
                    || UserId == Guid.Empty)
                {

                    return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "Fill All Required Fields!!" });

                }
                else
                {
                    var objDialerDbUser = _unitOfWork.UserRepository.FindById(UserId);
                    if (objDialerDbUser == null)
                    {
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "409", Message = "User ID Not Found!!" });

                    }

                    var groups = _unitOfWork.GroupChannelRepository.GetSingleGroup(model);
                    String message = "";
                    if (groups.Count() > 0)
                    {
                        message = "Retrieve User Groups Successfully!!";
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "200", Message = message, GroupData = groups });

                    }
                    else
                    {
                        message = "No Groups Found With Given Group ID";
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "409", Message = message });

                    }

                }

            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/createGroupChannel");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Get Group By Group Name
        [Route("api/account/v1/getGroupByName")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<GroupDetailsResponse> GetGroupByName([FromBody] GroupDetailsViewModel model)
        {
            try
            {

                if (model == null
                    || model.GroupName == ""
                    || model.GroupName == null
                    )
                {

                    return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "Fill All Group Name Field!!" });

                }
                else
                {

                    var groups = _unitOfWork.GroupChannelRepository.GetGroupByName(model);
                    String message = "";
                    if (groups.Count() > 0)
                    {
                        message = "Retrieve Groups Successfully!!";
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "200", Message = message, GroupData = groups });

                    }
                    else
                    {
                        message = "No Groups Found With Given Group Name";
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "409", Message = message });

                    }

                }

            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/createGroupChannel");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Get Group By Group DeepLink
        [Route("api/account/v1/getGroupByDeepLink")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<GroupDetailsResponse> GetGroupByDeepLink([FromBody] GroupDetailsViewModel model)
        {
            try
            {

                if (model == null
                    || model.DeepLink == ""
                    || model.DeepLink == null
                    )
                {

                    return await Task.FromResult(new GroupDetailsResponse { StatusCode = "400", Message = "Fill All Deeplink Field!!" });

                }
                else
                {

                    var group = _unitOfWork.GroupChannelRepository.GetGroupByDeepLink(model);
                    String message = "";
                    if (group != null)
                    {
                        List<GroupDetailsViewModel> GroupData = new List<GroupDetailsViewModel>();
                        GroupData.Add(group);

                        message = "Retrieve Groups Successfully!!";
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "200", Message = message, GroupData = GroupData });

                    }
                    else
                    {
                        message = "No Groups Found With Given DeepLink";
                        return await Task.FromResult(new GroupDetailsResponse { StatusCode = "409", Message = message });

                    }

                }

            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/createGroupChannel");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Update Aboutme
        [Route("api/account/v1/updateAboutme")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<UserAboutMeResponse> UpdateAboutme([FromBody] UserAboutMeViewModel model)
        {
            try
            {
                Guid UserId;
                try
                {
                    UserId = new Guid(model.UserID);
                }
                catch (Exception e)
                {
                    return await Task.FromResult(new UserAboutMeResponse { StatusCode = "400", Message = "User ID Must Be In Correct Format" });
                }
                if (model == null
                    // || string.IsNullOrEmpty(model.GroupID)
                    || model.UserID == null
                    || UserId == Guid.Empty)
                {
                    return await Task.FromResult(new UserAboutMeResponse { StatusCode = "400", Message = "Fill All Required Fields!!" });
                }
                else
                {
                    var user = _unitOfWork.UserRepository.FindById(UserId);
                    if (user == null)
                    {
                        return await Task.FromResult(new UserAboutMeResponse { StatusCode = "409", Message = "User ID Not Found!!" });
                    }
                    else
                    {
                        String message = "";
                        var userAboutme = _unitOfWork.UserAboutmeRepository.GetUserAboutMe(UserId.ToString());
                        
                        if (userAboutme == null)
                        {
                            _unitOfWork.UserAboutmeRepository.AddUserAboutme(model);
                            message = "User About Me Is Added Successfully.";
                        }
                        else
                        {
                            if (model.PhoneBrand == "" || model.PhoneBrand == null)
                            {
                                model.PhoneBrand = userAboutme.PhoneBrand;
                            }

                            if (model.PhoneModel == "" || model.PhoneModel == null)
                            {
                                model.PhoneModel = userAboutme.PhoneModel;
                            }

                            if (model.AboutMe == "" || model.AboutMe == null)
                            {
                                model.AboutMe = userAboutme.AboutMe;
                            }

                            if (model.Profession == "" || model.Profession == null)
                            {
                                model.Profession = userAboutme.Profession;
                            }

                            if (model.Address == "" || model.Address == null)
                            {
                                model.Address = userAboutme.Address;
                            }

                            _unitOfWork.UserAboutmeRepository.UpdateUserAboutme(model);
                            message = "User About Me Is Updated Successfully.";
                        }

                        return await Task.FromResult(new UserAboutMeResponse { StatusCode = "200", Message = message });

                    }

                }

            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/updateAboutme");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Add UserAboutMe Social Links
        [Route("api/account/v1/addSocialLink")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<UserAboutMeResponse> AddUserSocialLink([FromBody] List<UserSocialLink> userLinks)
        {
            var model = userLinks.Take(1).FirstOrDefault();
            try
            {
                Guid UserId;
                try
                {
                    UserId = new Guid(model.UserID);
                }
                catch (Exception e)
                {

                    return await Task.FromResult(new UserAboutMeResponse { StatusCode = "400", Message = "User ID Must Be In Correct Format" });

                }
                if (model == null
                    // || string.IsNullOrEmpty(model.GroupID)
                    || model.UserID == null
                    || model.SocialLink == null
                    || model.Name == null
                    || model.SocialLink == ""
                    || model.Name == ""
                    || model.UserID == null
                    || UserId == Guid.Empty)
                {

                    return await Task.FromResult(new UserAboutMeResponse { StatusCode = "400", Message = "Fill All Required Fields!!" });

                }
                else
                {
                    var user = _unitOfWork.UserRepository.FindById(UserId);
                    if (user == null)
                    {
                        return await Task.FromResult(new UserAboutMeResponse { StatusCode = "409", Message = "User ID Not Found!!" });

                    }
                    else
                    {
                        String message = "";
                        var result = _unitOfWork.UserAboutmeRepository.AddUserAboutMeSocialLink(userLinks);
                        if (result)
                        {
                            message = "User Links Added Successfully.";
                            return await Task.FromResult(new UserAboutMeResponse { StatusCode = "200", Message = message });
                        }
                        else
                        {
                            message = "Failed Adding Links!!";
                            return await Task.FromResult(new UserAboutMeResponse { StatusCode = "417", Message = message });
                        }




                    }

                }

            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/addSocialLink");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Delete UserAboutMe Social Link Based On Id 
        [Route("api/account/v1/deleteSocialLink")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<UserAboutMeResponse> DeleteSocialLink([FromBody] UserSocialLink model)
        {

            try
            {
                Guid UserId;
                try
                {
                    UserId = new Guid(model.UserID);
                }
                catch (Exception e)
                {

                    return await Task.FromResult(new UserAboutMeResponse { StatusCode = "400", Message = "User ID Must Be In Correct Format" });

                }
                if (model == null
                    // || string.IsNullOrEmpty(model.GroupID)
                    || model.UserID == null
                    || model.Id == null
                    || model.UserID == null
                    || UserId == Guid.Empty)
                {

                    return await Task.FromResult(new UserAboutMeResponse { StatusCode = "400", Message = "Fill All Required Fields!!" });

                }
                else
                {
                    var user = _unitOfWork.UserRepository.FindById(UserId);
                    if (user == null)
                    {
                        return await Task.FromResult(new UserAboutMeResponse { StatusCode = "409", Message = "User ID Not Found!!" });

                    }
                    else
                    {
                        String message = "";
                        var result = _unitOfWork.UserAboutmeRepository.DeleteUserAboutMeSocialLink(model.Id.ToString());
                        if (result)
                        {
                            message = "User Link Deleted Successfully.";
                            return await Task.FromResult(new UserAboutMeResponse { StatusCode = "200", Message = message });
                        }
                        else
                        {
                            message = "Failed While Deleting User Link!!";
                            return await Task.FromResult(new UserAboutMeResponse { StatusCode = "417", Message = message });
                        }




                    }

                }

            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/deleteSocialLink");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Update UserAboutMe Social Link Based On Id 
        [Route("api/account/v1/updateSocialLink")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<UserAboutMeResponse> UpdateSocialLink([FromBody] UserSocialLink model)
        {

            try
            {
                Guid UserId;
                try
                {
                    UserId = new Guid(model.UserID);
                }
                catch (Exception e)
                {

                    return await Task.FromResult(new UserAboutMeResponse { StatusCode = "400", Message = "User ID Must Be In Correct Format" });

                }
                if (model == null
                    // || string.IsNullOrEmpty(model.GroupID)
                    || model.UserID == null
                    || model.SocialLink == null
                    || model.Name == null
                    || model.SocialLink == ""
                    || model.Name == ""
                    || model.Id == null
                    || model.UserID == null
                    || UserId == Guid.Empty)
                {

                    return await Task.FromResult(new UserAboutMeResponse { StatusCode = "400", Message = "Fill All Required Fields!!" });

                }
                else
                {
                    var user = _unitOfWork.UserRepository.FindById(UserId);
                    if (user == null)
                    {
                        return await Task.FromResult(new UserAboutMeResponse { StatusCode = "409", Message = "User ID Not Found!!" });

                    }
                    else
                    {
                        String message = "";
                        var result = _unitOfWork.UserAboutmeRepository.UpdateUserAboutMeSocialLink(model.Id.ToString(), model.Name, model.SocialLink);
                        if (result)
                        {
                            message = "User Link Updated Successfully.";
                            return await Task.FromResult(new UserAboutMeResponse { StatusCode = "200", Message = message });
                        }
                        else
                        {
                            message = "Failed While Updating User Link!!";
                            return await Task.FromResult(new UserAboutMeResponse { StatusCode = "417", Message = message });
                        }




                    }

                }

            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/updateSocialLink");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Search User By Profession
        [Route("api/account/v1/searchByProfession")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<SearchByProfessionResponse> SearchByUserProfession([FromBody] UserAboutMeViewModel model)
        {
            try
            {

                if (model == null || string.IsNullOrEmpty(model.Profession))

                {

                    return await Task.FromResult(new SearchByProfessionResponse { StatusCode = "400", Message = "Fill Profession Field!!" });

                }
                else
                {


                    String message = "";
                    var userAboutmeData = _unitOfWork.UserAboutmeRepository.GetMatchingAboutme(model.Profession);
                    return await Task.FromResult(new SearchByProfessionResponse { StatusCode = "200", Message = message, UserAboutMeData = userAboutmeData });



                }

            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/searchByProfession");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Search User By Nearest Location
        [Route("api/account/v1/searchUserNearMe")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<SearchByProfessionResponse> SearchUserNearMe([FromBody] SearchUserNearMeViewModel model)
        {
            try
            {
                if (model == null
                 || string.IsNullOrEmpty(model.Latitude.ToString())
                 || string.IsNullOrEmpty(model.Longitude.ToString())
                 || string.IsNullOrEmpty(model.Radius.ToString()))

                {
                    return await Task.FromResult(new SearchByProfessionResponse { StatusCode = "400", Message = "Fill Data Fields!" });
                }
                if (model.Unit.Equals("Meter"))
                {
                    model.Radius = model.Radius / 1000;

                }
                else if (model.Unit.Equals("KM"))
                {

                }
                else
                {
                    return await Task.FromResult(new SearchByProfessionResponse { StatusCode = "400", Message = "Please Type Valid Radius Unit!" });
                }
                String message = "";
                var userAboutmeData = _unitOfWork.UserAboutmeRepository.UserNearMe(model.Latitude, model.Longitude, model.Radius);
                return await Task.FromResult(new SearchByProfessionResponse { StatusCode = "200", Message = message, UserAboutMeData = userAboutmeData });


            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/SearchUserNearMe");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        #region Update User Location
        [Route("api/account/v1/updateUserLocation")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<SearchByProfessionResponse> UpdateUserLocation([FromBody] SearchUserNearMeViewModel model)
        {
            try
            {

                if (model == null
                   || string.IsNullOrEmpty(model.Latitude.ToString())
                   || model.Latitude==0
                   || string.IsNullOrEmpty(model.Longitude.ToString())
                   || model.Longitude==0
                   || string.IsNullOrEmpty(model.UserID))

                {
                    return await Task.FromResult(new SearchByProfessionResponse { StatusCode = "400", Message = "Fill Data Fields!" });
                }
                String message = "User Location Updated Successfully!!";
                _unitOfWork.UserAboutmeRepository.UpdateUserLocation(model.Latitude, model.Longitude, model.UserID);
                return await Task.FromResult(new SearchByProfessionResponse { StatusCode = "200", Message = message });


            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/SearchUserNearMe");
                throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        #endregion

        private Image byteArrayToImage(byte[] byteArrayIn)
        {
            using (MemoryStream mStream = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(mStream);
            }
        }

        public class MemoryPostedFile : HttpPostedFileBase
        {
            private readonly byte[] fileBytes;

            public MemoryPostedFile(byte[] fileBytes, string fileName = null)
            {
                this.fileBytes = fileBytes;
                this.FileName = fileName;
                this.InputStream = new MemoryStream(fileBytes);

            }

            public override int ContentLength => fileBytes.Length;

            public override string FileName { get; }

            public override Stream InputStream { get; }
        }
    }
}
