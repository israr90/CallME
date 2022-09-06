using foneMe.SL.Entities;
using foneMe.SL.Interface;
using foneMe.ViewModels.Account;
using foneMe.ViewModels.Twilio;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Configuration;

namespace foneMe.DAL.Repositories
{
    internal class CallConnectionRepository : Repository<CallConnection>, ICallConnectionRepository
    {
        private readonly ApplicationDbContext _context;
        internal CallConnectionRepository(ApplicationDbContext context)
            : base(context)
        { _context = context; }

        public long MaxCount()
        {
            if (Set.Count() <= 0)
            {
                return 0;
            }
            else
            {
                return Set.Max(x => x.CallConnectionId);
            }
        }
        public long MaxCountLocalContact()
        {
            if (_context.LocalContacts.Count() <= 0)
            {
                return 0;
            }
            else
            {
                return _context.LocalContacts.Max(x => x.LocalContactsId);
            }
        }
        public CallConnectResponse SaveCallConnection(CallNotificationVM objCallNotificationVM)
        {
            try
            {
                var objCallConnection = new CallConnection();
                objCallConnection.CallConnectionId = MaxCount() + 1;
                objCallConnection.CallType = objCallNotificationVM.CallType;
                objCallConnection.CallDate = DateTime.Now;
                objCallConnection.StatusProfileId = objCallNotificationVM.StatusProfileId;
                objCallConnection.ReceiverNumber = objCallNotificationVM.ReceiverNumber;
                objCallConnection.ReceiverUserId = objCallNotificationVM.ReceiverId;
                objCallConnection.DialerUserId = objCallNotificationVM.UserId;
                objCallConnection.DialerNumber = objCallNotificationVM.DialerNumber;
                objCallConnection.CallNotificationProfileId = objCallNotificationVM.NotificationStatusProfileId;
                objCallConnection.ChannelName = objCallNotificationVM.ChannelName;
                objCallConnection.IsActive = true;
                objCallConnection.IsDeleted = false;
                _context.CallConnections.Add(objCallConnection);
                _context.SaveChanges();

                string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                // Get Call Connection Data
                var objCallConnectResponse = new CallConnectResponse();
                objCallConnectResponse.CallConnectionId = objCallConnection.CallConnectionId + "";
                objCallConnectResponse.DialerId = objCallNotificationVM.UserId;
                objCallConnectResponse.ReceiverId = objCallNotificationVM.ReceiverId;
                objCallConnectResponse.DialerNumber = objCallNotificationVM.DialerNumber;
                objCallConnectResponse.ReceiverNumber = objCallNotificationVM.ReceiverNumber;
                objCallConnectResponse.Status = objCallConnection?.Profile?.Name;
                objCallConnectResponse.NotificationType = objCallConnection?.Profile2?.ShortName;
                objCallConnectResponse.NotificationName = objCallConnection?.Profile2?.Name;
                objCallConnectResponse.CallType = objCallConnection.CallType;
                objCallConnectResponse.ChannelName = objCallConnection.ChannelName;
                objCallConnectResponse.CallDate = objCallConnection.CallDate;
                objCallConnectResponse.DeviceToken = objCallConnection?.User1?.DeviceToken;
                objCallConnectResponse.VOIPDeviceToken = objCallConnection?.User1?.FullName;
                if (objCallNotificationVM.ImageURL != null && objCallNotificationVM.ImageURL != "")
                {
                    objCallConnectResponse.DialerImageUrl = ProjectImgPth + objCallNotificationVM.ImageURL;
                }
                return objCallConnectResponse;
            }
            catch (Exception ex)
            {
                return new CallConnectResponse();
            }
        }


        public List<UserContacts> GetUserContactsPaged(UserContactsVM model)
        {
            try
            {
                string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];


                var lstUserContactsRespons = new List<UserContacts>();
                var cs = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;







                string queryString = "GetUserContactsPaged";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    using (SqlCommand cmd = new SqlCommand(queryString, con))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@userin", model.UserId);


                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var objUserContactsResponse = new UserContacts();
                                objUserContactsResponse.ContactsNumber = reader["PHONE"].ToString();
                                objUserContactsResponse.ContactsName = reader["NAME"].ToString();
                                objUserContactsResponse.ContactsFT = reader["DToken"].ToString();
                                objUserContactsResponse.ContactsVT = reader["VToken"].ToString();

                                //UPDATE01.07
                                objUserContactsResponse.ContactsCnic = reader["CNIC"] == null ? "" : reader["CNIC"].ToString();
                                //objUserContactsResponse.Image = ProjectImgPth + "/Common/DisplayImageById?fileId=" + reader["USERID"].ToString();
                                var image = reader["ImageURL"].ToString();
                                if (image != null && image != "")
                                {

                                    objUserContactsResponse.Image = ProjectImgPth + image;
                                }

                                lstUserContactsRespons.Add(objUserContactsResponse);

                            }

                        }
                    }
                }


                return lstUserContactsRespons;
            }
            catch (Exception ex)
            {
                return new List<UserContacts>();
            }
        }
        public List<UserContacts> GetUserMatchContactsbackup(UserContactsVM model)
        {
            try
            {
                string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                var objDbLocalContacts = _context.LocalContacts.Where(x => x.HaveContactsUserId == model.UserId)?.ToList();
                if (objDbLocalContacts?.Count() > 0)
                {
                    _context.LocalContacts.RemoveRange(objDbLocalContacts);
                    _context.SaveChanges();
                }
                var lstUserContactsRespons = new List<UserContacts>();
                var contactDetails = _context.Contacts.Where(x => x.IsActive == true && x.IsPrimary == true && x.Profile.ShortName == "MBN" && x.UserId != model.UserId)?.ToList();
                foreach (var item in model.Contacts)
                {
                    if (!string.IsNullOrEmpty(item.ContactsName))
                    {
                        if (!(item.ContactsNumber == "0"))
                        {
                            if (!(item.ContactsNumber == "+"))
                            {
                                foreach (var item1 in contactDetails)
                                {
                                    var com = item1.NumberWithOutCode != null ? item1.NumberWithOutCode : "";
                                    var com1 = item1.Description != null ? item1.Description : "";
                                    if (item.ContactsNumber.Contains(com1) || item.ContactsNumber.Contains(com) || com1.Contains(item.ContactsNumber) || com.Contains(item.ContactsNumber))
                                    {
                                        var objUserContactsResponse = new UserContacts();
                                        objUserContactsResponse.ContactsNumber = item1.Description;
                                        objUserContactsResponse.ContactsName = item.ContactsName;
                                        // objUserContactsResponse.Image = ProjectImgPth + "/Common/DisplayImageById?fileId=" + item1?.UserId;
                                        if (item1?.User.ImageURL != null && item1?.User.ImageURL != "")
                                        {
                                            objUserContactsResponse.Image = ProjectImgPth + item1?.User.ImageURL;

                                        }

                                        lstUserContactsRespons.Add(objUserContactsResponse);
                                        // Save User Contacts Name 
                                        var obj = new LocalContact();
                                        obj.LocalContactsId = MaxCountLocalContact() + 1;
                                        obj.HaveContactsUserId = model.UserId;
                                        obj.LocalContactUserId = item1?.UserId;
                                        obj.Name = item.ContactsName;
                                        _context.LocalContacts.Add(obj);
                                        _context.SaveChanges();
                                    }
                                }
                            }
                        }
                    }

                }
                return lstUserContactsRespons;
            }
            catch (Exception ex)
            {
                return new List<UserContacts>();
            }
        }

        public List<UserContacts> GetUserMatchContacts(UserContactsVM model)
        {
            try
            {
                string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];


                var lstUserContactsRespons = new List<UserContacts>();
                var cs = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;




                DataTable tvp = new DataTable();
                tvp.Columns.Add(new DataColumn("TITLE", typeof(string)));
                tvp.Columns.Add(new DataColumn("PHONE", typeof(string)));


                foreach (var incomingContact in model.Contacts)
                {
                    tvp.Rows.Add(incomingContact.ContactsName, incomingContact.ContactsNumber);

                }



                string queryString = "CheckUserExistsv2";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    using (SqlCommand cmd = new SqlCommand(queryString, con))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@userin", model.UserId);
                        //   cmd.Parameters.AddWithValue("@NAME", incomingContact.ContactsName);
                        //   cmd.Parameters.AddWithValue("@PHONE", incomingContact.ContactsNumber);

                        SqlParameter tvparam = cmd.Parameters.AddWithValue("@List", tvp);
                        // these next lines are important to map the C# DataTable object to the correct SQL User Defined Type
                        tvparam.SqlDbType = SqlDbType.Structured;
                        tvparam.TypeName = "dbo.ContactList";

                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var objUserContactsResponse = new UserContacts();
                                objUserContactsResponse.ContactsNumber = reader["PHONE"].ToString();
                                objUserContactsResponse.ContactsName = reader["NAME"].ToString();
                                objUserContactsResponse.ContactsFT = reader["DToken"].ToString();
                                objUserContactsResponse.ContactsFT = reader["VToken"].ToString();

                                //UPDATE01.07
                                objUserContactsResponse.ContactsCnic = reader["CNIC"] == null ? "" : reader["CNIC"].ToString();
                                //objUserContactsResponse.Image = ProjectImgPth + "/Common/DisplayImageById?fileId=" + reader["USERID"].ToString();
                                var image = reader["ImageURL"].ToString();
                                if (image != null && image != "")
                                {

                                    objUserContactsResponse.Image = ProjectImgPth + image;
                                }
                                lstUserContactsRespons.Add(objUserContactsResponse);

                            }

                        }
                    }
                }


                return lstUserContactsRespons;
            }
            catch (Exception ex)
            {
                return new List<UserContacts>();
            }
        }

        //private List<GetCallLogsModel> GetLeafNodes(List<CallLogStatu> ListFolderHierarchy, long parentNodeId, string ProjectImgPth)
        //{
        //    try
        //    {
        //        return ListFolderHierarchy.Where(l => l.CallLogStatusParentId == parentNodeId && l.IsActive == true & l.IsDeleted == false)
        //    .Select(l => new GetCallLogsModel
        //    {
        //        CallStartTime = l?.CallConnection?.CallStartTime,
        //        CallEndTime = l?.CallConnection?.CallEndTime,
        //        ReceiverName = l?.User?.FirstName,
        //        ReceiverImage = ProjectImgPth + "/Common/DisplayImageById?fileId=" + l?.CallUserId,
        //        ReceiverNumber = l?.MobileNumber,
        //        CallStatus = l?.Profile?.ShortName,
        //    }).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}

        //public List<GetCallLogsModel> GetCallConnectionLogs(Guid? UserId)
        //{
        //    try
        //    {
        //        string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
        //        var callLogStatus = new List<CallLogStatu>();
        //        var listCallLogStatus = new List<GetCallLogsModel>();
        //        callLogStatus = _context.CallLogStatus.ToList();

        //        listCallLogStatus = callLogStatus.Where(l => l.CallLogStatusParentId == 0 && l.CallUserId == UserId && l.IsActive == true & l.IsDeleted == false)
        //            .Select(l => new GetCallLogsModel
        //            {
        //                CallStartTime = l?.CallConnection?.CallStartTime,
        //                CallEndTime = l?.CallConnection?.CallEndTime,
        //                ReceiverName = l?.User?.FirstName,
        //                ReceiverImage = ProjectImgPth + "/Common/DisplayImageById?fileId=" + l?.CallUserId,
        //                ReceiverNumber = l?.MobileNumber,
        //                CallStatus = l?.Profile?.ShortName,
        //                LeafNodes = GetLeafNodes(callLogStatus, l.CallLogStatusId, ProjectImgPth)
        //            }).ToList();

        //        return listCallLogStatus;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new List<GetCallLogsModel>();
        //    }
        //}
        // Get Call Logs
        public List<GetCallLogsModel> GetCallConnectionLogs(Guid? userId)
        {
            try
            {
                string ProjectImgPth = WebConfigurationManager.AppSettings["ImageServiceURL"];
                var users = _context.CallConnections.Where(x => (x.DialerUserId == userId && x.ReceiverUserId != userId) || (x.DialerUserId != userId && x.ReceiverUserId == userId)
              && x.IsActive == true && x.IsDeleted == false)?.ToList();

                var lstGetCallLogsModel = new List<GetCallLogsModel>();
                foreach (var item in users)
                {
                    var objGetCallLogsModel = new GetCallLogsModel();
                    if (item.Profile?.ShortName == "OG" && item.DialerUserId == userId)
                    {


                        objGetCallLogsModel.CallLogNumber = item.ReceiverNumber;
                        //objGetCallLogsModel.CallLogImage = ProjectImgPth + "/Common/DisplayImageById?fileId=" + item.ReceiverUserId;
                       
                            if (item.User1!=null)
                            {

                                if (item.User1.ImageURL != "" && item.User1.ImageURL != null)
                                {
                                    objGetCallLogsModel.CallLogImage = ProjectImgPth + item.User1.ImageURL;
                                }
                            }
                        
                        objGetCallLogsModel.CallLogName = item.User1?.FirstName;
                        //objGetCallLogsModel.CallLogName = item.User1?.LocalContacts1?.Where(x => x.LocalContactUserId == item.User1?.UserId)?.FirstOrDefault()?.Name;
                        objGetCallLogsModel.CallStartTime = item.CallDate;
                        objGetCallLogsModel.CallEndTime = item.CallEndTime;
                        objGetCallLogsModel.CallLogStatus = item.Profile?.Name;

                        objGetCallLogsModel.CallingFoneId = item.User1?.LocalContacts1?.Where(x => x.LocalContactUserId == item.User1?.UserId)?.FirstOrDefault()?.User1.CNIC;
                        objGetCallLogsModel.CallingUserId = item.User1?.LocalContacts1?.Where(x => x.LocalContactUserId == item.User1?.UserId)?.FirstOrDefault()?.User1.UserId;


                        objGetCallLogsModel.CallerFoneId = item.User.CNIC;//.LocalContacts1?.Where(x => x.LocalContactUserId == item.User1?.UserId)?.FirstOrDefault()?.User1.CNIC; ;
                        objGetCallLogsModel.CallerUserId = item.User.UserId;//.?.LocalContacts1?.Where(x => x.LocalContactUserId == item.User1?.UserId)?.FirstOrDefault()?.User1.UserId; ;


                        lstGetCallLogsModel.Add(objGetCallLogsModel);
                    }
                    else
                    {
                        objGetCallLogsModel.CallLogNumber = item.DialerNumber;
                        //objGetCallLogsModel.CallLogImage = ProjectImgPth + "/Common/DisplayImageById?fileId=" + item.DialerUserId;

                        if (item.User1 != null)
                        {
                            if (item.User.ImageURL != "" && item.User.ImageURL != null)
                            {
                                objGetCallLogsModel.CallLogImage = ProjectImgPth + item.User.ImageURL;
                            }
                        }
                        
                      
                        objGetCallLogsModel.CallLogName = item.User?.FirstName;
                        //objGetCallLogsModel.CallLogName = item.User?.LocalContacts1?.Where(x => x.LocalContactUserId == item.User?.UserId)?.FirstOrDefault()?.Name;
                        objGetCallLogsModel.CallStartTime = item.CallDate;
                        objGetCallLogsModel.CallEndTime = item.CallEndTime;
                        objGetCallLogsModel.CallLogStatus = item.Profile1?.Name;
                        lstGetCallLogsModel.Add(objGetCallLogsModel);
                    }
                }
                return lstGetCallLogsModel;

                //var lstGetCallResponse = users?.Where(x => x.Profile?.ShortName == "OG")?.Count() > 0 ?
                //users?.Where(x => x.Profile?.ShortName == "OG" && x.DialerUserId == userId).Select(y => new GetCallLogsModel
                //{
                //    CallLogNumber = y.ReceiverNumber,
                //    CallLogImage = ProjectImgPth + "/Common/DisplayImageById?fileId=" + y.ReceiverUserId,
                //    CallLogName = y.User1?.FirstName,
                //    CallStartTime = y.CallDate,
                //    CallEndTime = y.CallEndTime,
                //    CallLogStatus = y.Profile.Name,

                //})?.ToList() : users?.Where(x => x.Profile?.ShortName == "OG" && x.DialerUserId == userId).Select(y => new GetCallLogsModel
                //{
                //    CallLogNumber = y.DialerNumber,
                //    CallLogImage = ProjectImgPth + "/Common/DisplayImageById?fileId=" + y.DialerUserId,
                //    CallLogName = y.User?.FirstName,
                //    CallStartTime = y.CallDate,
                //    CallEndTime = y.CallEndTime,
                //    CallLogStatus = y.Profile1.Name,

                //})?.ToList() ?? null;
            }
            catch (Exception ex)
            {
                return new List<GetCallLogsModel>();
            }
        }

        public bool AddUserasFriend(AddContactsVM model)
        {
            try
            {
                var cs = System.Configuration.ConfigurationManager.ConnectionStrings["foneMeCS"].ConnectionString;

                string queryString = "AddUserFriend";
                using (SqlConnection con = new SqlConnection(cs))
                {
                    using (SqlCommand cmd = new SqlCommand(queryString, con))
                    {
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@UserTitle", model.Name);
                        cmd.Parameters.AddWithValue("@LOCALID", model.FriendId);
                        cmd.Parameters.AddWithValue("@user", model.UserId);
                        //   cmd.Parameters.AddWithValue("@NAME", incomingContact.ContactsName);
                        //   cmd.Parameters.AddWithValue("@PHONE", incomingContact.ContactsNumber);



                        con.Open();
                        cmd.ExecuteNonQuery();



                    }
                }
                return true;

            }
            catch (Exception ex)
            {

                return false;
            }
        }
    }
}
