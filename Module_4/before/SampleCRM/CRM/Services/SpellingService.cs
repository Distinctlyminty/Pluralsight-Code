using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CRM.ViewModels;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck.Models;

namespace CRM.Services
{
    public class SpellingService
    {
        private string London = "Lat: 51.507; Long: 0.127; re:1000;";

        private string Key = "d334faeb8dd945bc83b60e77f7e4518d";

        public async Task<SpellingResult> CheckSpellingAsync(string text, string clientId = "")
        {
                var market = "en-GB";
                var geoLocation = London;
                var ipAddress = GetPublicIpAddress();

                SpellCheckClient client = new SpellCheckClient(new ApiKeyServiceClientCredentials(Key));

                var response = await
                     client.SpellCheckerWithHttpMessagesAsync(text, market: market,
                     clientId: clientId, clientIp: ipAddress, location: geoLocation);

            throw new NotImplementedException();
         
        }


        private string GetPublicIpAddress()
        {
            string publicIP = new System.Net.WebClient().DownloadString("https://api.ipify.org");
            return publicIP;
        }
    }

}
