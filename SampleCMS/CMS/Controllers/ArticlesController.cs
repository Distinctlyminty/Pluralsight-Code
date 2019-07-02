﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CMS.Helpers;
using CMS.ViewModels;

namespace CMS.Controllers
{
    public class ArticlesController : Controller
    {
        public ArticlesController()
        {

        }
        public ActionResult Index()
        {

            return View(GetIndexViewModel());
        }

        public ActionResult Create()
        {
            return View(new ArticleViewModel());
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            return View(MockDataStoreHelper.Get(id));
        }

        [HttpPost]
        public ActionResult Edit(ArticleViewModel model)
        {//TODO: FINISH THIS
            return View();
            //return View(MockDataStoreHelper.Get(id));
        }

        public ActionResult Details(int id)
        {
            return View(MockDataStoreHelper.Get(id));
        }

        public ActionResult Delete(int id)
        {
            MockDataStoreHelper.Delete(id);
            return RedirectToAction("index");
        }

        private IndexViewModel GetIndexViewModel()
        {
            return new IndexViewModel { Articles = MockDataStoreHelper.GetArticles() };

        }
    }
}