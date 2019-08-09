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

                var spellingResult = new SpellingResult
                {
                    ClientId = GetHeaderValue(response.Response.Headers, "X-MSEdge-ClientID"),
                    TraceId = GetHeaderValue(response.Response.Headers, "BingAPIs-SessionId"),
                    Text = ProcessResults(text, response.Body.FlaggedTokens)
                };
                return spellingResult;
         
        }

        private string GetHeaderValue(HttpResponseHeaders headers, string key)
        {
            string value = null;
            IEnumerable<string> returnedValue;
            if (headers.TryGetValues(key, out returnedValue))
            {
                value = returnedValue.FirstOrDefault();
            }

            return value;
        }

        private string ProcessResults(string text, IList<SpellingFlaggedToken> flaggedTokens)
        {
            StringBuilder newTextBuilder = new StringBuilder(text);

            int indexDiff = 0;

            foreach (var token in flaggedTokens)
            {
                    if (token.Type == "RepeatedToken")
                    {
                        newTextBuilder.Remove(token.Offset-indexDiff, token.Token.Length + 1);
                        indexDiff += token.Token.Length + 1;
                    }
                    else
                    {
                        if (token.Suggestions.Count > 0)
                        {
                            var suggestedToken = token.Suggestions.Where(x => x.Score >= 0.7).FirstOrDefault();
                            if (suggestedToken == null)
                                break;

                            // replace the token in the original text

                            newTextBuilder.Remove(token.Offset-indexDiff, token.Token.Length);
                            newTextBuilder.Insert(token.Offset-indexDiff, suggestedToken.Suggestion);

                            indexDiff += token.Token.Length - suggestedToken.Suggestion.Length;
                        }
                    }
               
            }
            return newTextBuilder.ToString();
        }


        private string GetPublicIpAddress()
        {
            string publicIP = new System.Net.WebClient().DownloadString("https://api.ipify.org");
            return publicIP;
        }
    }

}
