using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using news_engine.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.AspNet.Identity.EntityFramework;
using System.IO;
using System.Diagnostics;

namespace news_engine.Controllers
{
    public class ArticlesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Articles
        public ActionResult Index(int? page, string searchString)
        {
            var articles = from a in db.Articles select a;
            if (!String.IsNullOrEmpty(searchString))
            {
                articles = articles.Where(a => a.Title.Contains(searchString));
            }
            articles = articles.OrderByDescending(a => a.Date);
            int pageSize = 3;
            int pageNumber = 1;
            pageNumber = page.HasValue ? Convert.ToInt32(page) : 1;
            return View(articles.ToPagedList(pageNumber, pageSize));
        }

        // GET: Articles/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Article article = db.Articles.Find(id);
            if (article == null)
            {
                return HttpNotFound();
            }
            return View(article);
        }

        // GET: Articles/Create
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Articles/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Editor")]
        public ActionResult Create(Article article)
        {
            if (article.Title != null && article.Content != null && article.Thumbnail != null)
            {
                string imgName = Path.GetFileNameWithoutExtension(article.Thumbnail.FileName);
                string extension = Path.GetExtension(article.Thumbnail.FileName);
                imgName = imgName + DateTime.Now.ToString("yymmssff") + extension;
                article.ThumbnailUrl = imgName;
                article.Thumbnail.SaveAs(Path.Combine(Server.MapPath("~/App_Files/Images"), imgName));

                var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
                var user = manager.FindById(User.Identity.GetUserId());
                article.User = user;

                //var categoryName = article.Category.Name;
                //category = db.Categories.Where(i => i.Name == categoryName).FirstOrDefault();
                //category.Articles.
                db.Articles.Add(article);
                db.SaveChanges();
                var result = "Article creation successful";
                RedirectToAction("Index");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            var resultFailure = "Error";
            return Json(resultFailure, JsonRequestBehavior.AllowGet);

        }
        // GET: Articles/Edit/5
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Article article = db.Articles.Find(id);

            if (article == null)
            {
                return HttpNotFound();
            }
            if (article.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                IEnumerable<SelectListItem> categories = GetAllCategories();
                ViewBag.Categories = categories;
                return View(article);
            }
            else
            {
                TempData["message"] = "Permission denied";
 return RedirectToAction("Index");

            }

        }
        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from category in db.Categories select category;

            foreach (var category in categories)
            {
            
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.Name.ToString()
                });
            }

            return selectList;
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator,Editor")]
        public ActionResult Edit(Article article)
        {
            string path = "";
            var fileName = "";
            if (article.Thumbnail != null)
            {
                fileName = Path.GetFileName(article.Thumbnail.FileName);
                fileName = DateTime.Now.ToString("yymmddssff") + fileName;
                path = Path.Combine(Server.MapPath("~/App_Files/Images"), fileName);

                if (ModelState.IsValid)
                {
                    if (article.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                    {
                        article.Thumbnail.SaveAs(path);
                        article.ThumbnailUrl = fileName;
                        var newCategoryName = HttpContext.Request.Params.Get("newCategory");
                        int newCategoryId = Convert.ToInt32(newCategoryName);
                        article.CategoryId = newCategoryId;
                        Category newCategory = db.Categories.Find(newCategoryId);
                        
                        db.Entry(article).State = EntityState.Modified;
                        db.SaveChanges();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["message"] = "Permission denied";
                        return RedirectToAction("Index");
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        // GET: Articles/Delete/5
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Article article = db.Articles.Find(id);
            if (article == null)
            {
                return HttpNotFound();
            }
            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Article article = db.Articles.Find(id);
            db.Articles.Remove(article);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
