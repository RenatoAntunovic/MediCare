using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using MediCare.API.FCM;

public class FcmService : IFcmService
{
    private readonly FirebaseMessaging _messaging;

    public FcmService()
    {
        // uzima VEĆ inicijalizirani FirebaseApp
        _messaging = FirebaseMessaging.DefaultInstance;
    }

    public async Task<string> SendNotificationAsync(
        string fcmToken,
        string title,
        string body)
    {
        var message = new Message
        {
            Token = fcmToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };

        return await _messaging.SendAsync(message);
    }
}
