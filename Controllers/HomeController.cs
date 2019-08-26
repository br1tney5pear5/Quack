using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quack.Models;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Quack.Models;
using Quack.Models.Home;
using Quack.Models.Account;
using Microsoft.EntityFrameworkCore;


namespace Quack.Controllers
{
    public class HomeController : Controller
    {

        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ILogger<AccountController> _logger;
        private readonly QuackDbContext _context;

        public HomeController(UserManager<User> userManager,
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
        public async Task<IActionResult> Index()
        {
            var model = new IndexViewModel();
            int currentUserID = Convert.ToInt32(_userManager.GetUserId(HttpContext.User));
            UserDTO userDTO = await GetUserDTO(currentUserID);

            if(userDTO != null)
                model.userDTO = userDTO;

            return View(model);
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



        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
