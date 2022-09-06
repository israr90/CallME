using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace foneMe.ViewModels.Twilio
{
    public class VoiceTokenVM
    {

        public Guid UserId { get; set; }
        public string ChannelName { get; set; }
    }
    public class VoiceTokenResponse
    {
        public string StatusCode { get; set; }
        public string JWTToken { get; set; }

        public string JWTVoice { get; set; }
    }

    public class CallNotificationVM
    {

        public string DialerNumber { get; set; }
        public string ReceiverNumber { get; set; }
        public Guid UserId { get; set; }
        public String ImageURL { get; set; }
        public long? StatusProfileId { get; set; }
        public long? NotificationStatusProfileId { get; set; }
        public Guid ReceiverId { get; set; }
        public string ChannelName { get; set; }
        public string CallStatusType { get; set; }
        public string Status { get; set; }
        public string CallType { get; set; }
        public string AppType { get; set; }
        public string CallSid { get; set; }
    }

    public class CallNotificationResponse
    {

        public string StatusCode { get; set; }
        public string Message { get; set; }
    }
    public class CallConnectResponse
    {
        public string DialerNumber { get; set; }
        public string ReceiverNumber { get; set; }
        public string CallConnectionId { get; set; }
        public string DialerImageUrl { get; set; }
        public string DeviceToken { get; set; }
        public string CallerName { get; set; }
        public string DialerFoneID { get; set; }
        public string ReceiverFoneID { get; set; }
        public string VOIPDeviceToken { get; set; }
        public Guid? DialerId { get; set; }
        public Guid? ReceiverId { get; set; }
        public string ChannelName { get; set; }
        public DateTime? CallDate { get; set; }
        public string Status { get; set; }
        public string NotificationType { get; set; }
        public string NotificationName { get; set; }
        public string CallType { get; set; }
        public string AppType { get; set; }
        public string CallSid { get; set; }
    }

    public class CallStatusHandlingVM
    {
        public string CallConnectionId { get; set; }
        public string ReceiverStatus { get; set; }
        public Guid? ReceiverId { get; set; }
        public string NotificationType { get; set; }
        public DateTime CallReceivingTime { get; set; }
    }
    public class UserPushNotifications
    {
        public Guid? ReceiverUserId { get; set; }
        public string SenderMobileNumber { get; set; }
        public string NotificationType { get; set; }
    }
    public class CallDialerNotification
    {
        public string NotificationType { get; set; }
        public string NotificationName { get; set; }
        public string SenderMobileNumber { get; set; }
        public string DeviceToken { get; set; }
    }
    
    public class GetCallLogVM
    {
        public Guid? UserId { get; set; }
    }
    public class GetCallLogsModel
    {
        //public List<GetCallLogsModel> LeafNodes { get; set; }
        public string CallLogNumber { get; set; }
        public Guid? CallerUserId { get; set; }
        public Guid? CallingUserId { get; set; }
        public string CallingFoneId { get; set; }
        public string CallerFoneId { get; set; }
        public string CallLogName { get; set; }
        public string CallLogImage { get; set; }
        public DateTime? CallStartTime { get; set; }
        public DateTime? CallEndTime { get; set; }
        public string CallLogStatus { get; set; }
    }
    public class GetCallResponse
    {
        public List<GetCallLogsModel> CallLogs { get; set; }
        public string StatusCode { get; set; }
    }
}
