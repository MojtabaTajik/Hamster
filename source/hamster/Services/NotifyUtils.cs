using System.Net;

namespace hamster.Services;

public class NotifyUtils
{
    public static async Task<bool> SendNotification(string service, string title, string message, string level)
    {
        try
        {
            var uri = new Uri(
                $"https://botops.dgab.dev/api/notifications/send-alert?title={title}&message={message}&level={level}&service={service}");

            var getResult = await new HttpClient().GetAsync(uri);
            return getResult.StatusCode == HttpStatusCode.OK;
        }
        catch(Exception ex)
        {
            return false;
        }
    }
}