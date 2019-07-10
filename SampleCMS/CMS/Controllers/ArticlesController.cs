using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CMS.Helpers;
using CMS.Services;
using CMS.ViewModels;

namespace CMS.Controllers
{
    public class ArticlesController : Controller
    {
        ArticleService _articleService;
        public ArticlesController()
        {
            _articleService = new ArticleService();
        }
        public ActionResult Index()
        {

            return View(GetIndexViewModel());
        }

        public ActionResult Create()
        {
            return View(new ArticleViewModel());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(ArticleViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model).WithWarning("Error", "Please correct the errors and try again");
                }
                MockDataStoreHelper.Add(model);
            }
            catch (Exception)
            {
                return View(model).WithWarning("Error", "Unable to save record");
            }


            return RedirectToAction("Index").WithSuccess("Completed", "Record Saved");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            return View(MockDataStoreHelper.Get(id));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(ArticleViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model).WithWarning("Error", "Please correct the errors and try again");
                }
                MockDataStoreHelper.Update(model);
            }
            catch (Exception)
            {
                return View(model).WithWarning("Error", "Unable to update record");
            }


            return RedirectToAction("Index").WithSuccess("Completed", "Record Updated");


        }

        public ActionResult Details(int id)
        {
            // this is going to show article stats
            var detailsViewModel = GetDetailsViewModel(id);
            return View(detailsViewModel);
        }

        private DetailsViewModel GetDetailsViewModel(int id)
        {
            DetailsViewModel model = new DetailsViewModel();
            var article = MockDataStoreHelper.Get(id);

            var info = _articleService.GetArticleInfo(article);
            model.Article = article;
            model.Info = info;
            return model;

        }

        public ActionResult Delete(int id)
        {
            MockDataStoreHelper.Delete(id);
            return RedirectToAction("index").WithSuccess("Deleted", "Record deleted");
        }

        private IndexViewModel GetIndexViewModel()
        {
            return new IndexViewModel { Articles = MockDataStoreHelper.GetArticles() };

        }
    }
}