using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CMS.ViewModels
{
    public class ArticleViewModel
    {
        public int Id { get; set; }

        [DataType(DataType.Text)]
        [Required]
        public string Title { get; set; }

       [Required]
       [DataType(DataType.MultilineText)]
       [AllowHtml]
        public string Body { get; set; }


    }
}