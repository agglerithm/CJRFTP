namespace CJRFTP.Common
{ 

    public interface INotificationSender
    {
        void SendMessage(string subject, string recipients, string body);
    }

   
}
