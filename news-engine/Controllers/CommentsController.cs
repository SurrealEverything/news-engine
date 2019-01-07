using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using news_engine.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace news_engine.Controllers
{
    [Route("Articles/Comments")]
    public class CommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Comments
        public ActionResult Index(int? page, int articleId)
        {
            var comments = from a in db.Comments select a;
            comments = comments.Where(c => c.ArticleId.Equals(articleId));
            comments = comments.OrderByDescending(c => c.Date);
            int pageSize = 3;
            int pageNumber = 1;
            pageNumber = page.HasValue ? Convert.ToInt32(page) : 1;
            return View(comments.ToPagedList(pageNumber, pageSize));
        }

        // GET: Comments/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Comments/Create
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Comment comment)
        {
            if (comment.Message != null)
            {
                var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
                var user = manager.FindById(User.Identity.GetUserId());
                comment.User = user;

                db.Comments.Add(comment);
                db.SaveChanges();
                var result = "Comment added successfully";
                RedirectToAction("Index");
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            var resultFailure = "Error";
            return Json(resultFailure, JsonRequestBehavior.AllowGet);
        }

        /*
        // POST: Comments/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        */

        // GET: Comments/Edit/5
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);

            if (comment == null)
            {
                return HttpNotFound();
            }
            if (comment.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
            {
                return View(comment);
            }
            else
            {
                TempData["message"] = "Permission denied";
                return RedirectToAction("Index");

            }

        }
        /*
        // POST: Articles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Comment comment)
        {
            if (ModelState.IsValid)
            {
                if (comment.UserId == User.Identity.GetUserId() || User.IsInRole("Administrator"))
                {
                    db.Entry(comment).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["message"] = "Permission denied";
                    return RedirectToAction("Index");
                }


            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        */

        // GET: Comments/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // GET: Articles/Delete/5
        [Authorize(Roles = "Editor,Administrator")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }
    }
}
