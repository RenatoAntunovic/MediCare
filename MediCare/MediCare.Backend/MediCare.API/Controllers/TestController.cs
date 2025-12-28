using MediCare.API.FCM;

[ApiController]
[Route("api/test")]
public class TestController : ControllerBase
{
    private readonly IFcmService _fcmService;

    public TestController(IFcmService fcmService)
    {
        _fcmService = fcmService;
    }

    [HttpGet("send")]
    public async Task<IActionResult> SendTest()
    {
        string realFcmToken = "cPtAg_xFy4J0_J6_2VVvYF:APA91bEvNppiYsMnwP9NaCW9xiUOUxEKm-Odjv8ahC2OMHvP4Bx76qARK3_-woN8RFyuaCbHhLcQ327Co43jvzLN97Iwz58mrFTzsE8He4LzhV5JBCkCDNs";

        var result = await _fcmService.SendNotificationAsync(
            realFcmToken,                 // ✅ TOKEN
            "Test notifikacija",           // ✅ TITLE
            "Ovo je test poruka"           // ✅ BODY
        );

        return Ok(result);
    }
}
