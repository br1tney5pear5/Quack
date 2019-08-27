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

        Func<Post, int?, bool>
            ofUserFilter = ((p, ofUserID) => ofUserID.HasValue
                            ?  p.authorID == ofUserID
                            : true); 

        Func<Post, List<int>, bool>
            forUserFilter = ((p , followed) => followed != null
                             ? followed.Any(f => f == p.authorID)
                             : true);

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
        public async Task<IEnumerable<PostDTO>> GetPosts(int? skip,
                                                         int? count,
                                                         int? ofUserID,
                                                         int? forUserID)
        {
            return await _GetPosts((p => true), count, ofUserID, forUserID);
        }


        [HttpPost]
        public async Task<IEnumerable<PostDTO>> GetPostsBefore(int ID,
                                                               int? count,
                                                               int? ofUserID,
                                                               int? forUserID)
        {
            return await _GetPosts((p => p.ID < ID), count, ofUserID, forUserID);
        }

        [HttpPost]
        public async Task<IEnumerable<PostDTO>> GetPostsAfter(int ID,
                                                              int? maxcount,
                                                              int? ofUserID,
                                                              int? forUserID)
        {
            return await _GetPosts((p => p.ID > ID), maxcount, ofUserID, forUserID);
        }

        private async Task<IEnumerable<PostDTO>> _GetPosts(Func<Post, bool> idFilter,
                                                           int? count,
                                                           int? ofUserID,
                                                           int? forUserID)
        {
            List<int> followed = !forUserID.HasValue ? null : await _context.Following
                .Where(f => f.followerID == forUserID.Value)
                .Select(f => f.followedID)
                .ToListAsync();

            count = count ?? 100;
            return await _context.Post
                .OrderByDescending(p => p.datePublished)
                //I have absolutely no idea why I cant just pass lambda here
                .Where(p => idFilter(p))
                .Where(p => ofUserFilter(p, ofUserID)) 
                .Where(p => forUserFilter(p, followed))
                .Take(count.Value)
                .Include(p => p.content)
                .Include(p => p.author)
                .Include("comments.author")
                .Select(p => new PostDTO(p))
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Bread() {
            return View (await GetUserDTO
                 (Convert.ToInt32(_userManager.GetUserId(HttpContext.User))));
        }

        [HttpPost]
        public async Task<IActionResult> FollowUser(int ID) {
            int currentUserID = Convert.ToInt32(_userManager.GetUserId(HttpContext.User));
            if(await _userManager.FindByIdAsync(ID.ToString()) != null &&
               !_context.Following.Any(f => f.followerID == currentUserID && f.followedID == ID))
            {
                var following = new Following {
                    followerID = currentUserID,
                    followedID = ID
                };
                _context.Following.Add(following);
                _context.SaveChanges();

                var user = await _userManager.FindByIdAsync(ID.ToString());

                TempData["message"] = "You are following " + user.UserName;
            } else {
                TempData["message"] = "Something went wrong";
            }

            return RedirectToAction("User", "Account", new { ID });
        }

        [HttpPost]
        public async Task<IActionResult> UnfollowUser(int ID) {
            int currentUserID = Convert.ToInt32(_userManager.GetUserId(HttpContext.User));
            Func<Following, bool> condition =
                (f => f.followerID == currentUserID && f.followedID == ID);

            if(await _userManager.FindByIdAsync(ID.ToString()) != null &&
               _context.Following.Any(condition))
            {
                _context.Following.Remove(_context.Following.FirstOrDefault(condition));

                _context.SaveChanges();

                var user = await _userManager.FindByIdAsync(ID.ToString());

                TempData["message"] = "Unfollowed " + user.UserName;
            } else {
                TempData["message"] = "Something went wrong";
            }

            return RedirectToAction("User", "Account", new { ID });
        }


        [HttpGet]
        public async Task<IActionResult> User(int ID) {
            var model = new UserViewModel{
                userDTO = await GetUserDTO(ID),
                currentUserDTO = await GetCurrentUserDTO()
            };

            if(model.userDTO != null)
                return View(model);

            TempData["message"] = "There is no such user!";
            return RedirectToAction("Index", "Home");
        }

        private async Task<UserDTO> GetUserDTO(int ID) {
            var user = await _userManager.FindByIdAsync(ID.ToString());

            if(user != null) {
                var userDTO = new UserDTO(user);
                userDTO.postsCount = _context.Post.Count(p => p.authorID == ID);
                userDTO.commentsCount = _context.Comment.Count(p => p.authorID == ID);
                userDTO.followed = _context.Following.Any(f =>
                                                          f.followerID == Convert.ToInt32(_userManager.GetUserId(HttpContext.User)) && f.followedID == ID);
                userDTO.followingCount =
                    _context.Following.Count(f => f.followerID == ID);
                userDTO.followedByCount =
                    _context.Following.Count(f => f.followedID == ID);

                return userDTO;
            }
            return null;
        }

        private async Task<UserDTO> GetCurrentUserDTO() {
              return await GetUserDTO(Convert.ToInt32(_userManager.GetUserId(HttpContext.User)));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null) {
            if(_signInManager.IsSignedIn(HttpContext.User)) {
                TempData["message"] = "You are already logged in";
                return RedirectToAction("Index", "Home");
            }
            ViewData["returnUrl"] = returnUrl;
            ViewBag.returnUrl = returnUrl;
            return View();

            // var tcs = new TaskCompletionSource<IActionResult>();
            // tcs.SetException(new NotImplementedException());
            // return tcs.Task;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() {
            TempData["message"] = "No real emails - No password recovery, sorry.";
            return RedirectToAction("Login", "Account");
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null) {
            if(!ModelState.IsValid){
                return View(model);
                // RedirectToAction("Login", "Account");
            }
            _logger.LogWarning(model.username);
            _logger.LogWarning(model.password);
            _logger.LogWarning(model.rememberMe.ToString());
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
            if(_signInManager.IsSignedIn(HttpContext.User)) {
                TempData["message"] = "You are already a registered user";
                return RedirectToAction("Index", "Home");
            }
            ViewData["returnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(string text, int postID, string returnUrl) {
            if(ModelState.IsValid) {
                var comment = new Comment{
                    postID = postID,
                    text = text,
                    authorID = Convert.ToInt32(_userManager.GetUserId(HttpContext.User)),
                    datePublished = DateTime.UtcNow
                };
                _context.Comment.Add(comment);
                await _context.SaveChangesAsync();
            }
            return Redirect(returnUrl); //security vuln?
        }

        private IActionResult RedirectToLocal(string returnUrl) {
            _logger.LogWarning(returnUrl);
            if(Url.IsLocalUrl(returnUrl)) {
                return Redirect(returnUrl);
            }else {
                return RedirectToAction("Index", "Home");

            }
        }


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
                TempData["message"] = "Something went Wrong :/";
                return View(model);
            }
            var user = new User{ UserName = model.username, Email = model.email };
            var result = await _userManager.CreateAsync(user, model.password);
            if(result.Succeeded) {
                await _signInManager.SignInAsync(user, isPersistent: false);

                TempData["message"] = "Hey, you're now a Quack user!";
                return RedirectToAction("Index", "Home");
            }

            TempData["message"] = "Something went Wrong :/";
            return View(model);
        }

        private void AddErrors(IdentityResult result) {
            foreach(var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
