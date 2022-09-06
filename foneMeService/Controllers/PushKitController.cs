using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using PushSharp.Apple;
using PushSharp.Core;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using foneMe.ViewModels.Account;
using System.Web.Configuration;
using System.Web.Hosting;
using foneMe.ViewModels.Twilio;

namespace foneMeService.Controllers
{

    public class PushKitController : ApiController
    {
        [Route("api/pushKit/v1/sendvoipnotification")]
        [AllowAnonymous]
        [HttpPost]
        public async Task<BoolResultVM> SendVoipNotification([FromBody] FirebaseVM model)
        {
            try
            {
                if (model == null)
                {
                    return await Task.FromResult(new BoolResultVM { IsSuccessed = false, StatusCode = "400" });
                }
                var result = SendVOIPNotification(model.DeviceToken);
                return await Task.FromResult(new BoolResultVM { IsSuccessed = true, StatusCode = "200", Data = result });
            }
            catch (Exception ex)
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Error(ex, "Exception at api/account/v1/sendvoipnotification");
                return await Task.FromResult(new BoolResultVM { IsSuccessed = false, StatusCode = "Exception: " + ex.ToString(), Data = ex.Message });
                //throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }
        }
        public string SendVOIPNotification(string dT)
        {
            string pushkitVOIPCertificate = WebConfigurationManager.AppSettings["PushKitVOIPCertificate"];
            string pushkitVOIPCertificatePassword = WebConfigurationManager.AppSettings["PushKitVOIPCertificatePassword"];
            var p12fileName = HostingEnvironment.MapPath(@pushkitVOIPCertificate);
            //string p12fileName = "C:\\webroot\\PKI\\myCertificate.p12";
            string p12password = pushkitVOIPCertificatePassword;
            string deviceToken = dT;
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
            dynamic jsonObject = new JObject();
            jsonObject.DialerId = DateTime.Now;
            jsonObject.ReceiverId = "Me Against the world";
            jsonObject.DialerNumber = 1995;
            jsonObject.ReceiverNumber = "2Pac";
            jsonObject.NotificationType = "2Pac";
            jsonObject.CallType = "2Pac";
            jsonObject.CallDate = "2Pac";
            jsonObject.Status = "2Pac";
            jsonObject.ChannelName = "2Pac";
            jsonObject.DeviceToken = "2Pac";
            jsonObject.CallLogStatusId = "2Pac";
            jsonObject.DialerImageUrl = "2Pac";


            CallConnectResponse result = new CallConnectResponse();
            result.DialerId = Guid.NewGuid();
            result.ReceiverId = Guid.NewGuid();
            result.DialerNumber = "dassada";
            result.ReceiverNumber = "4334343";
            result.NotificationType = "CLLCN";
            result.CallType = "NT";
            result.Status = "NT";
            result.ChannelName = "NT";
            result.DeviceToken = "NT";
            result.CallConnectionId = "232";
            result.DialerImageUrl = "https://www.image.com";
            var Data = new Dictionary<string, string>()
                    {
                     { "DialerId", 1+"" },
                     { "ReceiverId", 2+"" },
                     { "DialerNumber", result.DialerNumber+"" },
                     { "ReceiverNumber", result.ReceiverNumber+"" },
                     { "NotificationType", result.NotificationType+"" },
                     { "CallType", result.CallType+"" },
                     { "CallDate", result.CallDate+"" },
                     { "Status", result.Status+"" },
                     { "ChannelName", result.ChannelName+"" },
                     { "DeviceToken", result.DeviceToken+"" },
                     { "CallLogStatusId", result.CallConnectionId+"" },
                     { "DialerImageUrl", result.DialerImageUrl },
                     };
            apnsBroker.Start();
            apnsBroker.QueueNotification(new ApnsNotification
            {
                DeviceToken = deviceToken,
                //Payload = jsonObject
                Payload = JObject.Parse("{\"aps\":{\"alert\":\"" + "Hi,, This Is a Sample Push Notification For IPhone.." + "\",\"badge\":1,\"DialerId\":\"" + result.DialerId + "\",\"ReceiverId\":\"" + result.ReceiverId + "\",\"DialerNumber\":\"" + result.DialerNumber + "\",\"ReceiverNumber\":\"" + result.ReceiverNumber + "\",\"NotificationType\":\"" + result.NotificationType + "\",\"CallType\":\"" + result.CallType + "\",\"CallDate\":\"" + DateTime.Now.ToShortDateString() + "\",\"Status\":\"" + result.Status + "\",\"ChannelName\":\"" + result.ChannelName + "\",\"DeviceToken\":\"" + result.DeviceToken + "\",\"CallLogStatusId\":" + result.CallConnectionId + ",\"DialerImageUrl\":\"" + result.DialerImageUrl + "\",\"sound\":\"default\"}}")
                // Payload = JObject.Parse("{\"aps\":{\"alert\":\"" + "Hi,, This Is a Sample Push Notification For IPhone.." + "\",\"badge\":1,\"DialerId\":" + 2 + ",\"ReceiverId\":1,\"DialerNumber\":2,\"ReceiverNumber\":2,\"NotificationType\":\"" + "Mango" + "\",\"CallType\":2,\"CallDate\":2,\"Status\":2,\"ChannelName\":2,\"DeviceToken\":2,\"CallLogStatusId\":2,\"DialerImageUrl\":2,\"sound\":\"default\"}}")
            });
            var val = "";
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
                        val = statusCode + "";
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
                val = "send";
                Console.WriteLine("Apple Notification Sent!");
            };
            //apnsBroker.OnNotificationFailed += (ss,sasd) => {
            //    val = "failed";
            //    Console.WriteLine("Apple Notification Sent!");
            //};

            apnsBroker.Stop();
            return val;
        }


        //public void SendVOIPNotification(string dT)
        //{
        //    string p12fileName = "D:\\QRTS\\Clients\\VoipsCert\\Voips_Cert.p12";
        //    //string p12fileName = "C:\\webroot\\PKI\\myCertificate.p12";
        //    string p12password = "1234567890";
        //    string deviceToken = dT;
        //    var appleCert = System.IO.File.ReadAllBytes(p12fileName);
        //    var config = new PushSharp.Apple.ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, appleCert, p12password, false);
        //    config.ValidateServerCertificate = false;
        //    var logger = NLog.LogManager.GetCurrentClassLogger();

        //    var apnsBroker = new ApnsServiceBroker(config);
        //    apnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
        //    {
        //        aggregateEx.Handle(ex =>
        //        {
        //            // See what kind of exception it was to further diagnose           
        //            if (ex is ApnsNotificationException)
        //            {
        //                var notificationException = (ApnsNotificationException)ex;
        //                // Deal with the failed notification               
        //                var apnsNotification = notificationException.Notification;
        //                var statusCode = notificationException.ErrorStatusCode;
        //                Console.WriteLine("Apple Notification Failed: ID={" + apnsNotification.Identifier + "}, Code={" + statusCode + "}");
        //            }
        //            else
        //            {
        //                // Inner exception might hold more useful information like an ApnsConnectionException   
        //                Console.WriteLine("Notification Failed for some unknown reason : {" + ex.InnerException + "}");
        //            }
        //            // Mark it as handled           
        //            return true;
        //        });
        //    };
        //    dynamic jsonObject = new JObject();
        //    jsonObject.DialerId = DateTime.Now;
        //    jsonObject.ReceiverId = "Me Against the world";
        //    jsonObject.DialerNumber = 1995;
        //    jsonObject.ReceiverNumber = "2Pac";
        //    jsonObject.NotificationType = "2Pac";
        //    jsonObject.CallType = "2Pac";
        //    jsonObject.CallDate = "2Pac";
        //    jsonObject.Status = "2Pac";
        //    jsonObject.ChannelName = "2Pac";
        //    jsonObject.DeviceToken = "2Pac";
        //    jsonObject.CallLogStatusId = "2Pac";
        //    jsonObject.DialerImageUrl = "2Pac";

        //    apnsBroker.Start();
        //    apnsBroker.QueueNotification(new ApnsNotification
        //    {
        //        DeviceToken = deviceToken,
        //        //Payload = jsonObject
        //        Payload = JObject.Parse("{\"aps\":{\"alert\":\"" + "Hi,, This Is a Sample Push Notification For IPhone.." + "\",\"badge\":1,\"DialerId\":" + 2 + ",\"ReceiverId\":1,\"DialerNumber\":2,\"ReceiverNumber\":2,\"NotificationType\":\"" + "Mango" + "\",\"CallType\":2,\"CallDate\":2,\"Status\":2,\"ChannelName\":2,\"DeviceToken\":2,\"CallLogStatusId\":2,\"DialerImageUrl\":2,\"sound\":\"default\"}}")
        //    });
        //    var val = "";
        //    apnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
        //    {
        //        aggregateEx.Handle(ex =>
        //        {
        //            // See what kind of exception it was to further diagnose
        //            if (ex is ApnsNotificationException)
        //            {
        //                var notificationException = (ApnsNotificationException)ex;

        //                // Deal with the failed notification
        //                var apnsNotification = notificationException.Notification;
        //                var statusCode = notificationException.ErrorStatusCode;

        //                Console.WriteLine($"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}");
        //            }
        //            else
        //            {
        //                // Inner exception might hold more useful information like an ApnsConnectionException          
        //                Console.WriteLine($"Apple Notification Failed for some unknown reason : {ex.InnerException}");
        //            }
        //            // Mark it as handled
        //            return true;
        //        });
        //    };
        //    apnsBroker.OnNotificationSucceeded += (notification) =>
        //    {
        //        val = "send";
        //        Console.WriteLine("Apple Notification Sent!");
        //    };
        //    //apnsBroker.OnNotificationFailed += (ss,sasd) => {
        //    //    val = "failed";
        //    //    Console.WriteLine("Apple Notification Sent!");
        //    //};

        //    apnsBroker.Stop();

        //}

    }


}





