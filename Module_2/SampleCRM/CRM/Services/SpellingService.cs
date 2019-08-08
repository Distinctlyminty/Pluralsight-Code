using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CRM.ViewModels;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck;
using Microsoft.Azure.CognitiveServices.Language.SpellCheck.Models;
using Microsoft.Rest;

namespace CRM.Services
{
    public class SpellingService
    {
        SpellCheckClient _client;
        string _key = "API_KEY";

        public async Task<string> CheckSpellingAsync(ArticleViewModel model)
        {

                                                                                                                                                                                                                                
        }

    }

}
