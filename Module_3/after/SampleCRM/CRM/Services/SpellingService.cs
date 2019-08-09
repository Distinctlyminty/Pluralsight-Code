using System.Threading.Tasks;
using CRM.ViewModels;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck;

namespace CRM.Services
{
    public class SpellingService
    {
        private string London = "Lat: 51.507; Long: 0.127; re:1000;";

        private string Key = "d334faeb8dd945bc83b60e77f7e4518d";

        public async Task<string> CheckSpellingAsync(ArticleViewModel model)
        {

            var market = "en-GB";
            var clientId = ""; // we don't have this yet
            var geoLocation = London;
            var ipAddress = GetPublicIpAddress();

            SpellCheckClient client = new SpellCheckClient(new ApiKeyServiceClientCredentials(Key));

           var response = await
                client.SpellCheckerWithHttpMessagesAsync(model.Body, market: market,
                clientId: clientId, clientIp: ipAddress, location: geoLocation);

            return await response.Response.Content.ReadAsStringAsync();
        }

        private string GetPublicIpAddress()
        {
            string publicIP = new System.Net.WebClient().DownloadString("https://api.ipify.org");
            return publicIP;

        }

    }

}
