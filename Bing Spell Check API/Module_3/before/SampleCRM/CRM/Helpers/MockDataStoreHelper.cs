using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using CRM.ViewModels;

namespace CRM.Helpers
{
    public static class MockDataStoreHelper
    {
        private static readonly ICollection<ArticleViewModel> _articles = new List<ArticleViewModel>();

        static MockDataStoreHelper()
        {
            Seed();
        }

        public static ICollection<ArticleViewModel> GetArticles()
        {
            return _articles;
        }

        public static ArticleViewModel Get(int id)
        {
            return _articles.FirstOrDefault(x => x.Id == id);
        }

        public static void Add(ArticleViewModel article)
        {
            _articles.Add(article);
        }

        public static void Update(ArticleViewModel article)
        {
            var temp = _articles.ToDictionary(x => x.Id);

            ArticleViewModel toUpdate;
            if (temp.TryGetValue(article.Id, out toUpdate))
            {
                toUpdate.Title = article.Title;
                toUpdate.Body = article.Body;
            }
        }

        private static void Seed()
        {
            // seed initial test data

            _articles.Add(new ArticleViewModel()
            {
                Id = 1,
                Title = "Improve your work life balance",
Body = "Did you know that all of our employees now have the option to work from home?"
            });
            _articles.Add(new ArticleViewModel()
            {
                Id = 2,
                Title = "Are you getting 5 a day?",
                Body = "Daily fruit deliveries are now being made to all of our office worldwide!"
            });
            _articles.Add(new ArticleViewModel()
            {
                Id = 3,
                Title = "Security is everyones responsibility",
                Body = "if you recieve an external email - do not open attachments unless you were expecting them"
            });
            _articles.Add(new ArticleViewModel()
            {
                Id = 4,
                Title = "Check before you send",
                Body =
                    "Please check all content twice before sending"
            });
            _articles.Add(new ArticleViewModel()
            {
                Id = 5,
                Title = "Embarrasing article casts us in a bad light",
                Body = "Embarrasing headlines today as news spreads of poorly written social media content."
            });
        }
    }
}