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
using Microsoft.EntityFrameworkCore;

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

        [HttpPost]
        public async Task<IEnumerable<CommentDTO>> GetPostComments(int ID) {
            return await _context.Comment
                .OrderByDescending(p => p.datePublished)
                .Where(c => c.postID == ID)
                .Include(c => c.author)
                .Select(c => new CommentDTO(c))
                .ToListAsync();
        }

        [HttpPost]
        public async Task<IEnumerable<PostDTO>> GetPosts(int? skip, int? count) {
            skip = skip ?? 0;
            count = count ?? 10;
            return await _context.Post
                .OrderByDescending(p => p.datePublished)
                .Skip(skip.Value)
                .Take(count.Value)
                .Include(p => p.content)
                .Include(p => p.author)
                .Include("comments.author")
                .Select(p => new PostDTO(p))
                .ToListAsync();
        }

        [HttpPost]
        public async Task<IEnumerable<PostDTO>> GetPostsBefore(int ID, int? count) {
            count = count ?? 10;
            return await _context.Post
                .OrderByDescending(p => p.datePublished)
                .Where(p => p.ID < ID)
                .Take(count.Value)
                .Include(p => p.content)
                .Include(p => p.author)
                .Include("comments.author")
                .Select(p => new PostDTO(p))
                .ToListAsync();
        }

        [HttpPost]
        public async Task<IEnumerable<PostDTO>> GetPostsAfter(int ID, int? maxcount) {
            maxcount = maxcount ?? 100;
            return await _context.Post
                .OrderByDescending(p => p.datePublished)
                .Where(p => p.ID > ID)
                .Take(maxcount.Value)
                .Include(p => p.content)
                .Include(p => p.author)
                .Include("comments.author")
                .Select(p => new PostDTO(p))
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> User(int ID) {
            var user = await _userManager.FindByIdAsync(ID.ToString());

            if(user != null) {
                var userDTO = new UserDTO(user);
                return View(userDTO);
            }

            TempData["message"] = "There is no such user!";
            return RedirectToAction("Index", "Home");
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
                TempData["message"] = "The username and password you entered did not match the records. Please double-check and try again.";
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
                    authorID = Convert.ToInt32(_userManager.GetUserId(HttpContext.User)),
                    datePublished = DateTime.UtcNow
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
