using System;
using System.ComponentModel;
using CMS.Services;

namespace CMS.ViewModels
{
    public class DetailsViewModel
    {
        public ArticleViewModel Article { get; set; }

        public ArticleInfoViewModel Info { get; set; }
    }
}
