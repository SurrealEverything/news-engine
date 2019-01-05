using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using news_engine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace news_engine.Controllers
{
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
                    var selectedRole =
                    db.Roles.Find(HttpContext.Request.Params.Get("newRole"));
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

    }
}
