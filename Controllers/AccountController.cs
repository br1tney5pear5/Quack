using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Quack.Models;
using Quack.Models.Account;

namespace Quack.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly QuackDbContext _context;

        public AccountController(UserManager<User> userManager,
                                 SignInManager<User> signInManager,
                                 ILogger<AccountController> logger,
                                 QuackDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null) {
            ViewData["returnUrl"] = returnUrl;
            ViewBag.returnUrl = returnUrl;
            return View();

            // var tcs = new TaskCompletionSource<IActionResult>();
            // tcs.SetException(new NotImplementedException());
            // return tcs.Task;
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null) {
            if(!ModelState.IsValid){
                _logger.LogWarning("1");
                return View(model);
                // RedirectToAction("Login", "Account");
            }
            var result = await _signInManager.PasswordSignInAsync(model.username,
                                                                  model.password,
                                                                  model.rememberMe,
                                                                  lockoutOnFailure: false);
            if(!result.Succeeded) {
                ViewBag.failureMessage = "The username and password you entered did not match the records. Please double-check and try again.";
                return View(model);
            }

            _logger.LogWarning("3");
            return RedirectToAction("Index", "Home");
            // return RedirectLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout() {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null) {
            ViewData["returnUrl"] = returnUrl;
            return View();
        }

        // [HttpGet]
        // [AllowAnonymous]
        // public Task<IActionResult> GetFeed(string returnUrl = null) {
        //     IQueryable<Post> postQuery = _context.Posts
        //         .Include(p => p.content);

        //     var PostFeed = new PostFeedViewModel();
        //     PostFeed.posts = await postQuery.ToListAsync();

        //     if(PostFeed.posts[0].content == null) {
        //         _logger.LogWarning("content isis null");
        //     }

        //     var model = new HomeViewModel{
        //         postFeedViewModel = PostFeed
        //     };
        //     // foreach(var post in PostFeed.Posts) {
        //     //     _logger.LogWarning(post.ID.ToString());
        //     // }

        //     return View(model);
        // }

        [HttpPost]
        public async Task<IActionResult> AddPost(PostContent model) {
            if(ModelState.IsValid) {
                var post = new Post{
                    content = model,
                    authorID = Convert.ToInt32(_userManager.GetUserId(HttpContext.User))
                };

                _context.Post.Add(post);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Home");
        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model) {
            if(!ModelState.IsValid){
                _logger.LogWarning("FAILED TO REGISTER");
                return View(model);
            }
            var user = new User{ UserName = model.username, Email = model.email };
            var result = await _userManager.CreateAsync(user, model.password);
            if(result.Succeeded) {
                _logger.LogInformation("Created new account {model.Email}");
                await _signInManager.SignInAsync(user, isPersistent: false);
                return View(model);
            }
            _logger.LogWarning("FAILED TO REGISTER");
            AddErrors(result);
            return View(model);
        }

        private void AddErrors(IdentityResult result) {
            foreach(var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }

        private IActionResult RedirectLocal(string url) {
            if(Url.IsLocalUrl(url))
                return RedirectToAction(url);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }
}
