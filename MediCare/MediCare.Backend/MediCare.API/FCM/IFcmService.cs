namespace MediCare.API.FCM
{
    public interface IFcmService
    {
        Task<string> SendNotificationAsync(string fcmToken, string title, string body);
    }

}
