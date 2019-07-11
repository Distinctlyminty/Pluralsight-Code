using System;
using System.Text.RegularExpressions;
using CMS.ViewModels;

namespace CMS.Services
{
    public class ArticleService
    {
        public ArticleInfoViewModel GetArticleInfo(ArticleViewModel article)
        {
            ArticleInfoViewModel info = new ArticleInfoViewModel();
            
            info.WordCount = WordCount(article.Body);
            info.CharCount = article.Body.Length;
            return info;
        }

        private int WordCount(string text)
        {
            int wordCount = 0, index = 0;

            // skip whitespace until first word
            while (index < text.Length && char.IsWhiteSpace(text[index]))
                index++;

            while (index < text.Length)
            {
                // check if current char is part of a word
                while (index < text.Length && !char.IsWhiteSpace(text[index]))
                    index++;

                wordCount++;

                // skip whitespace until next word
                while (index < text.Length && char.IsWhiteSpace(text[index]))
                    index++;
            }
            return wordCount;
        }
    }
}
