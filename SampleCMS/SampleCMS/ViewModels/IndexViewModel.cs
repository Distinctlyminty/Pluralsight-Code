using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SampleCMS.ViewModels
{
    public class IndexViewModel
    {
        public ICollection<ArticleViewModel> Articles { get; set; }
    }
}