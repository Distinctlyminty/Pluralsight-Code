using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CMS.ViewModels;

namespace CMS.Helpers
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

        public static void Delete(int id)
        {
            var article = _articles.FirstOrDefault(x => x.Id == id);
            if (article != null)
            {
                _articles.Remove(article);
            }
        }

        private static void Seed()
        {
            // seed initial test data

            _articles.Add(new ArticleViewModel()
            {
                Id = 1,
                Title = "Big Corp announces annual profits",
                Body = "London - 25th October \n Big Corp announces annual profits."
            });
            _articles.Add(new ArticleViewModel()
            {
                Id = 2,
                Title = "NEW CEO Announced",
                Body = "After much anticipation, Big Corp today announce Mr Smith as there new CEO"
            });
            _articles.Add(new ArticleViewModel()
            {
                Id = 3,
                Title = "Widgets now avvailable in 100 countries worldwide",
                Body = "Big Corp today announed that widgets will be availabile in 100 countries worldwide"
            });
            _articles.Add(new ArticleViewModel()
            {
                Id = 4,
                Title = "New Content System being rolled out",
                Body =
                    "Today we announce that we are rolling out a new IT system that will reduce errors in our content"
            });
            _articles.Add(new ArticleViewModel()
            {
                Id = 5,
                Title = "Embarrasing article casts big corp in a bad light",
                Body = "Embarrasing headlines for BigCorp today as news spreads of poorly written social media content."
            });
        }
    }
}