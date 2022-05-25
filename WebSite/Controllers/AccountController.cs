using Microsoft.AspNetCore.Mvc;
using WebSite.Models;
using WebSite.DatabaseAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebSite.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }

        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public async Task<IActionResult> Login(LoginModel lm)
        {
            /*
            if (string.IsNullOrEmpty(lm.UserName) || string.IsNullOrEmpty(lm.UserPwd))
            {
                return Content("<script>alert('账号密码不能为空');location.href='#'</script>");
            }*/
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "登录异常");
                return View();
            }

            var user = await userManager.FindByNameAsync(lm.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "用户不存在");
                return View();
            }

            var result = await signInManager.PasswordSignInAsync(user, lm.UserPwd, false, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "用户名或密码错误");
            return View(lm);

            /*
            //这里写个方法去数据库判断登录名和密码是否正确
            if (lm.UserName == "mike" && lm.UserPwd == "123456")
            {
                var claimIdentity = new ClaimsIdentity("Cookie");
                claimIdentity.AddClaim(new Claim(ClaimTypes.Name, lm.UserName));
                var claimPrincipal = new ClaimsPrincipal(claimIdentity);
                HttpContext.SignInAsync(claimPrincipal);
                return RedirectToAction("Index");
            }
            else
            {
                //ModelState.AddModelError(model.KeyValidateMessage, "登录名或密码错误(登录名:mike,密码:123456)");
                //return RedirectToAction("Login", new { ErrorMessage = "用户名密码不正确" });
                return Content("<script>alert('账号密码不能为空');location.href='#'</script>");
            }

            return View("~/Views/Account/Login.cshtml");*/
        }

        public async Task<IActionResult> Logout()
        {
            //HttpContext.SignOutAsync();
            //return Redirect("/");
            await signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
    }
}
