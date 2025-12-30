using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MediCare.Application.Modules.FCM.Services
{
    public class FcmService
    {
        private readonly HttpClient _httpClient;
        private readonly string _serverKey;

        public FcmService(HttpClient httpClient, string serverKey)
        {
            _httpClient = httpClient;
            _serverKey = serverKey; // iz Firebase Console -> Project Settings -> Cloud Messaging -> Server Key
        }

        public async Task<bool> SendNotificationAsync(string fcmToken, string title, string body, string orderId)
        {
            var message = new
            {
                to = fcmToken,
                notification = new
                {
                    title,
                    body
                },
                data = new
                {
                    orderId
                }
            };

            var json = JsonSerializer.Serialize(message);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send");
            request.Headers.TryAddWithoutValidation("Authorization", $"key={_serverKey}");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
