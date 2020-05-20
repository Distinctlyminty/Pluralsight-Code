using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CRM.ViewModels;
using Microsoft.Rest;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck;

namespace CRM.Services
{
    public class SpellingService
    {
        private string London = "Lat: 51.507; Long: 0.127; re:1000;";
        private string Key = "86463f82092a4859a2c5250b6f1e36d0";

        public async Task<string> CheckSpellingAsync(ArticleViewModel model)
        {
            var market = "en-US";
            var clientId = ""; // we don't have this yet
            var geoLocation = London;
            var ipAddress = GetPublicIpAddress();

            SpellCheckClient client = new SpellCheckClient(new ApiKeyServiceClientCredentials(Key));

            var response = await
                            client.SpellCheckerWithHttpMessagesAsync(model.Body, market: market,
                            clientId: clientId, clientIp: ipAddress, location: geoLocation,mode: "spell");

            return await response.Response.Content.ReadAsStringAsync();

        }

        private string GetPublicIpAddress()
        {
            string publicIP = new System.Net.WebClient().DownloadString("https://api.ipify.org");
            return publicIP;
        }
    }

}
