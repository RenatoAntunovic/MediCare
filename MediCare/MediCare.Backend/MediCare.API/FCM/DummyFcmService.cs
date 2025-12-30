namespace MediCare.API.FCM
{
    public class DummyFcmService : IFcmService
    {
        public Task<string> SendNotificationAsync(string title, string body, string token)
        {
            // Možeš vratiti bilo kakav string, ovo je dummy implementacija
            return Task.FromResult("Dummy notification sent");
        }
    }
}
