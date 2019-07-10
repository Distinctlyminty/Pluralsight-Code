using System;
using System.ComponentModel;

namespace CMS.Services
{
    public class ArticleInfoViewModel
    {
       [DisplayName("Word Count")]
        public int WordCount { get; set; }

        [DisplayName("Charachter Count")]
        public int CharCount { get; set; }

    }
}
