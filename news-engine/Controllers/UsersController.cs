using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using news_engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace news_engine.Controllers
{
    public static class UserGetter
    {
        public static ApplicationUser GetApplicationUser(this System.Security.Principal.IIdentity identity)
        {
            if (identity.IsAuthenticated)
            {
                using (var db = new ApplicationDbContext())
                {
                    var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
                    return userManager.FindByName(identity.Name);
                }
            }
            else
            {
                return null;
            }
        }
    }
    
    public class UserWithRole
    {
        public ApplicationUser User { get; set; }
        public string Role { get; set; }

        public UserWithRole(ApplicationUser User, string Role)
        {
            this.User = User;
            this.Role = Role;
        }
    }

    [Authorize(Roles = "Administrator")]
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Users
        public ActionResult Index()
        {
            var users = from u in db.Users select u;
            var roles = db.Roles;
            List<UserWithRole> usersWithRoles = new List<UserWithRole>();
            foreach (var user in users)
            {
                string userRole = roles.Find(user.Roles.First().RoleId).Name;
                usersWithRoles.Add(new UserWithRole(user, userRole));
            }
            ViewBag.Users = usersWithRoles;
            
            return View();
        }

        // GET: Users/Edit/5
        public ActionResult Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;
            return View(user);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();
            var roles = from role in db.Roles select role;
            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }

        [HttpPut]
        public ActionResult Edit(string id, ApplicationUser newData)
        {
            ApplicationUser user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            var userRole = user.Roles.FirstOrDefault();
            ViewBag.userRole = userRole.RoleId;

            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var roleManager = new RoleManager<IdentityRole>(new
               RoleStore<IdentityRole>(context));
                var UserManager = new UserManager<ApplicationUser>(new
               UserStore<ApplicationUser>(context));

                if (TryUpdateModel(user))
                {
                    user.UserName = newData.UserName;
                    user.Email = newData.Email;
                    var roles = from role in db.Roles select role;
                    foreach (var role in roles)
                    {
                        UserManager.RemoveFromRole(id, role.Name);
                    }
                    var selectedRole = db.Roles.Find(HttpContext.Request.Params.Get("newRole"));
                    UserManager.AddToRole(id, selectedRole.Name);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                return View(user);
            }

        }
        // POST: /Users/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            if (ModelState.IsValid)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }

                var _userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(db));
                var user = await _userManager.FindByIdAsync(id);
                var logins = user.Logins;
                var articles = user.Articles;
                var rolesForUser = await _userManager.GetRolesAsync(id);

                using (var transaction = db.Database.BeginTransaction())
                {
                    foreach (var login in logins.ToList())
                    {
                        await _userManager.RemoveLoginAsync(login.UserId, new UserLoginInfo(login.LoginProvider, login.ProviderKey));
                    }

                    if (rolesForUser.Count() > 0)
                    {
                        foreach (var item in rolesForUser.ToList())
                        {
                            // item should be the name of the role
                            var result = await _userManager.RemoveFromRoleAsync(user.Id, item);
                        }
                    }

                    if (articles.Count > 0)
                    {
                        foreach (var article in articles.ToList())
                        {
                            db.Articles.Remove(article);
                            await db.SaveChangesAsync();
                        }
                    }

                    await _userManager.DeleteAsync(user);
                    transaction.Commit();
                }

                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }
    }
}
