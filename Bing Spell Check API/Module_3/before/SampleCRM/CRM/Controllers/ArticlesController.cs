using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CRM.Helpers;
using CRM.Services;
using CRM.ViewModels;

namespace CRM.Controllers
{
    public class ArticlesController : Controller
    {
        SpellingService _spellingService;

        public ArticlesController()
        {
            _spellingService = new SpellingService();
        }
        public ActionResult Index()
        {
            return View(GetIndexViewModel());
        }

        [HttpGet]
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
                var response =  _spellingService.CheckSpellingAsync(model).Result;

                if (!string.IsNullOrEmpty(response))
                {
                    model.SpellingResponse = response;
                    return View(model).WithInfo("Info", "We have checked the spelling of this article");

                }

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

        private IndexViewModel GetIndexViewModel()
        {
            return new IndexViewModel { Articles = MockDataStoreHelper.GetArticles() };

        }
    }
}