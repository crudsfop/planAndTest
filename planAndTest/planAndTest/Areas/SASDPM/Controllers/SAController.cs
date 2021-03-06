﻿using commonLib;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Razor.Language;
using modelsfwk.SA;
using planAndTest.Helper;
using planAndTest.Helper.PM;
using planAndTest.Helper.SA;
using planAndTest.Models.SA;
using SASDdb.entity.fwk;
//using SASDdb.entity.Models;
using SASDdbService;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace planAndTest.Areas.SASDPM.Controllers
{
    public class SAController : Controller
    {
        const string EMPTY_PARENT_TITLE = "(root)";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Articles(string articleId, string parentDirId)
        {
            articlesViewModel viewModel = new articlesViewModel();
            string err = "";
            err = loadArticle(articleId, parentDirId, ref viewModel);
            viewModel.errorMsg = err;
            //viewModel.articleId = articleId;
            //todo !!...(5) full text search for articles
            //(1) when click on a subject in subject list: 1. need to show currently selected subject in content pane
            //2. need to enable edit/reply to two button (disabled before then)
            // special layout dir(left top), subject(right top), content(bottom most left), relation link (bottom rightmost)

            // first level cannot create article, allow only project(s), one project one article
            return View(viewModel);
        }

        private string loadArticle(string articleId, string parentDirId
            , ref articlesViewModel viewModel)
        {
            string ret = "";
            // load directories
            tblArticle tbart = new tblArticle();
            article parent = null;
            viewModel.subjects = new List<article>();
            article art = null;
            if (!string.IsNullOrWhiteSpace(articleId))
            {
                art = tbart.GetArticleById(articleId);
                if (art != null)
                {
                    viewModel.articleTitle = art.articleTitle;
                    viewModel.articleType = art.articleType;
                    viewModel.articleHtmlContent = art.articleHtmlContent;
                    if (string.IsNullOrWhiteSpace(parentDirId))
                        parentDirId = art.belongToArticleDirId.ToString();
                }
            }
            if (art==null)
            {
                viewModel.articleTitle = "";
                viewModel.articleType = "";
                viewModel.articleHtmlContent = "";
            }
            if (!string.IsNullOrWhiteSpace(parentDirId))
                parent = tbart.GetArticleById(parentDirId);
            if (parent == null)
            {
                viewModel.parentDirId = "";
                viewModel.parentDirTitle = "";
            }
            else
            {
                viewModel.parentDirId = parent.articleId.ToString();
                viewModel.parentDirTitle = parent.articleTitle;
                parent.belongToArticleDirId = parent.articleId;
                viewModel.subjects.Add(parent);
            }
            viewModel.directories = tbart.directoriesByParentArticleId(parentDirId);
            // load subjects
            viewModel.subjects.AddRange(tbart.subjectsByParentArticleId(parentDirId));
            // the article of the current directory, should be listed at the top of the subject list
            // and view its content when click on it
            return ret;
        }
        [HttpPost]
        public ActionResult Articles(articlesViewModel viewModel)
        {
            ActionResult ar;
            var selectedDirectory = Request.Form["selectedDirectory"];
            var selectedArticle = Request.Form["selectedArticle"];
            if (!string.IsNullOrWhiteSpace(selectedDirectory))
            {
                string[] dirs = selectedDirectory.Split(',');
                viewModel.selectedDirId = new List<string>(dirs);
            }
            if (!string.IsNullOrWhiteSpace(selectedArticle))
            {
                string[] arts = selectedArticle.Split(',');
                viewModel.selectedArticleId = new List<string>(arts);
            }
            var confirmDelete = TempData["confirmDelete"];
            var tmpVM = TempData["articlesViewModel"];
            articlesViewModel tmpViewModel = null;
            if (tmpVM != null)
            {
                tmpViewModel = (articlesViewModel)tmpVM;
                if (confirmDelete != null && (int)confirmDelete == 1)
                {
                    viewModel.selectedDirId = tmpViewModel.selectedDirId;
                    viewModel.selectedArticleId = tmpViewModel.selectedArticleId;
                }
            }
            string err = loadArticle(viewModel.articleId, viewModel.parentDirId, ref viewModel);
            viewModel.errorMsg = err;
            articleEditViewModel aevm;
            tblArticle ta;
            article art;
            switch (viewModel.cmd)
            {
                case "create":
                    aevm = new articleEditViewModel();
                    if (!string.IsNullOrWhiteSpace(viewModel.parentDirId))
                    {
                        aevm.editModel.belongToArticleDirId = Guid.Parse(viewModel.parentDirId);
                        ta = new tblArticle();
                        art = ta.GetArticleById(viewModel.parentDirId);
                        aevm.parentDirTitle = art.articleTitle;
                    }
                    aevm.changeMode = ARTICLE_CHANGE_MODE.CREATE;
                    aevm.pageStatus = (int)modelsfwk.PAGE_STATUS.ADD;
                    TempData["articleEditViewModel"] = aevm;
                    ar = RedirectToAction("EditArticle");
                    return ar;
                case "edit":
                    ta = new tblArticle();
                    art = ta.GetArticleById(viewModel.articleId);
                    //aevm = jsonUtl.decodeJson<articleEditViewModel>(jsonUtl.encodeJson(art));
                    aevm = new articleEditViewModel();
                    aevm.editModel = art;
                    if (art == null || art.belongToArticleDirId == null)
                        aevm.parentDirTitle = EMPTY_PARENT_TITLE;
                    else
                    {
                        article artParent = ta.GetArticleById(art.belongToArticleDirId.ToString());
                        aevm.parentDirTitle = artParent.articleTitle;
                    }
                    aevm.changeMode = ARTICLE_CHANGE_MODE.EDIT;
                    aevm.pageStatus = (int)modelsfwk.PAGE_STATUS.EDIT;
                    TempData["articleEditViewModel"] = aevm;
                    ar = RedirectToAction("EditArticle");
                    return ar;
                case "replyTo":
                    aevm = new articleEditViewModel();
                    aevm.editModel.belongToArticleDirId = new Guid(viewModel.articleId);
                    aevm.parentDirTitle = viewModel.articleTitle;
                    aevm.changeMode = ARTICLE_CHANGE_MODE.REPLY_TO;
                    aevm.pageStatus = (int)modelsfwk.PAGE_STATUS.ADD;
                    TempData["articleEditViewModel"] = aevm;
                    ar = RedirectToAction("EditArticle");
                    return ar;
                case "parentDir":
                    ar = RedirectToAction("Articles", new { articleId = viewModel.parentDirId });
                    return ar;
                case "delete":
                    // delete confirm
                    if (viewModel.selectedDirId.Count > 0 || viewModel.selectedArticleId.Count > 0)
                    {
                        ViewBag.confirmDelete = "1";
                        TempData["confirmDelete"] = 1;
                    }
                    ar = View(viewModel);
                    break;
                case "realDelete":
                    // to real delete 
                    //SASDdbBase db = new SASDdbBase();
                    string err1 = "";
                    tblArticle tart = new tblArticle();
                    DbContextTransaction transaction = tart.BeginTransaction();
                    try
                    {
                        foreach (string articleId in viewModel.selectedDirId)
                        {
                            err1 = tart.DeleteArticleOrDir(articleId);
                            if (err1.Length > 0)
                                throw new Exception(err1);
                        }
                        foreach (string articleId in viewModel.selectedArticleId)
                        {
                            err1 = tart.DeleteArticleOrDir(articleId);
                            if (err1.Length > 0)
                                throw new Exception(err1);
                        }
                        tart.SaveChanges();
                        transaction.Commit();
                        viewModel.successMsg = "selected directory(s) or article(s) successfully deleted";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        viewModel.errorMsg += ex.Message;
                        viewModel.successMsg = "";
                    }
                    err = loadArticle(viewModel.articleId, viewModel.parentDirId, ref viewModel);
                    viewModel.errorMsg += err;
                    // at last no matter what result, clear selection
                    viewModel.selectedDirId.Clear();
                    viewModel.selectedArticleId.Clear();
                    ar = View(viewModel);
                    break;
                default:
                    ar = View(viewModel);
                    break;
            }
            TempData["articlesViewModel"] = viewModel;
            return ar;
        }

        public ActionResult EditArticle()//string isDir)
        {
            articleEditViewModel viewModel;
            var tmpvar = TempData["articleEditViewModel"];
            if (tmpvar != null)
                viewModel = (articleEditViewModel)tmpvar;
            else
            {
                viewModel = new articleEditViewModel();
                viewModel.editModel.priority = 5;
            }
            //if (!string.IsNullOrWhiteSpace(isDir) && isDir == "1")
            //    viewModel.isDir = 1;
            //else
            //    viewModel.isDir = 0;
            //ViewBag.isDir = viewModel.isDir;
            if (viewModel.editModel.belongToArticleDirId == null)
                viewModel.parentDirTitle = EMPTY_PARENT_TITLE;
            // if editing, article/directory cannot be changed
            ViewBag.articleTypeOption = SAdropdownOptions.articleTypeOption();
            ViewBag.articleStatusOption = SAdropdownOptions.articleStatusOption();
            ViewBag.articlePriorityOption = SAdropdownOptions.articlePriorityOption();
            ViewBag.userList = PMdropdownOption.userList();
            TempData["articleEditViewModel"] = viewModel;
            return View(viewModel);
        }
        private string checkForm(articleEditViewModel viewModel)
        {
            string ret = "";
            if (string.IsNullOrWhiteSpace(viewModel.editModel.articleTitle))
                ret = "article (or directory) title cannot be empty";
            return ret;
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult EditArticle(articleEditViewModel viewModel)
        {
            ActionResult ret;
            ViewBag.articleTypeOption = SAdropdownOptions.articleTypeOption();
            ViewBag.articleStatusOption = SAdropdownOptions.articleStatusOption();
            ViewBag.articlePriorityOption = SAdropdownOptions.articlePriorityOption();
            ViewBag.userList = PMdropdownOption.userList();
            string err;
            // articles, ckeditor, paste base64 image
            switch (viewModel.cmd)
            {
                case "save":
                    err = checkForm(viewModel);
                    if (err.Length > 0)
                    {
                        viewModel.errorMsg = err;
                        ret = View(viewModel);
                        break;
                    }
                    //article article2add = new article();
                    //article2add.ArticleId = Guid.NewGuid();
                    //article2add.ArticleTitle = 
                    //    viewModel.ArticleTitle;
                    //article2add.ArticleHtmlContent = 
                    //    viewModel.ArticleHtmlContent;
                    string pureText;
                    err = htmlHelper.removeHtmlTags(
                        viewModel.editModel.articleHtmlContent, out pureText);
                    if (err.Length > 0)
                    {
                        viewModel.errorMsg = err;
                        ret = View(viewModel);
                        break;
                    }
                    viewModel.editModel.articleContent = pureText;
                    //article2add.IsDir = viewModel.IsDir ;
                    tblArticle tArticle = new tblArticle();
                    if (viewModel.changeMode == ARTICLE_CHANGE_MODE.CREATE)
                    {
                        viewModel.editModel.articleId = Guid.NewGuid();
                        viewModel.editModel.createtime = DateTime.Now;
                        //article art = new article();
                        //art.articleId = viewModel.articleId;
                        //art.createtime = DateTime.Now;
                        //art.articleTitle = viewModel.articleTitle;
                        //art.articleHtmlContent = viewModel.articleHtmlContent;
                        //art.articleContent = viewModel.articleContent;
                        //art.isDir = viewModel.isDir;
                        //art.belongToArticleDirId = viewModel.belongToArticleDirId;

                        err = tArticle.Add(viewModel.editModel);// as article);
                        err += tArticle.SaveChanges();
                        if (err.Length > 0)
                            viewModel.errorMsg = "error: " + err;
                        else
                        {
                            viewModel.successMsg = "new article successfully added";
                            viewModel.pageStatus = (int)modelsfwk.PAGE_STATUS.ADDSAVED;
                        }
                    }
                    else if (viewModel.changeMode == ARTICLE_CHANGE_MODE.EDIT)
                    {
                        err = tArticle.Update(viewModel.editModel);
                        err += tArticle.SaveChanges();
                        if (err.Length > 0)
                            viewModel.errorMsg = "error: " + err;
                        else
                        {
                            viewModel.successMsg = "article successfully updated";
                            viewModel.pageStatus = (int)modelsfwk.PAGE_STATUS.SAVED;
                        }
                    }
                    else if (viewModel.changeMode == ARTICLE_CHANGE_MODE.REPLY_TO)
                    {
                        // transaction, 1. create replied article 2. change original article to be directory type
                        string err1 = "";
                        SASDdbBase db = new SASDdbBase();
                        using (var transaction = db.BeginTransaction())
                        {
                            viewModel.editModel.articleId = Guid.NewGuid();
                            viewModel.editModel.createtime = DateTime.Now;
                            err1 = tArticle.Add(viewModel.editModel);// as article);
                            err1 += tArticle.SaveChanges();
                            tblArticle tart = new tblArticle();
                            article replied = tart.GetArticleById(viewModel.editModel.belongToArticleDirId.ToString());
                            replied.isDir = true;
                            err1 += tart.Update(replied);
                            err1 += tart.SaveChanges();
                            if (err1.Length == 0)
                                transaction.Commit();
                            else
                                err += err1;
                        }
                        if (err.Length > 0)
                            viewModel.errorMsg = "error: " + err;
                        else
                        {
                            viewModel.successMsg = "new article successfully added";
                            viewModel.pageStatus = (int)modelsfwk.PAGE_STATUS.ADDSAVED;
                        }
                    }
                    // notification failed, so, should use pure hidden field rather than html helped 
                    //ViewBag.Message = "article/directory saved";                    
                    ret = View(viewModel);
                    break;
                default:
                    ret = View(viewModel);
                    break;
            }
            TempData["articleEditViewModel"] = viewModel;
            return ret;
        }
    }
}