using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SimpleCloudStorage.Models;

namespace SimpleCloudStorage.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private UserManager<IdentityUser> _userManager;
        private SignInManager<IdentityUser> _signInManager;
        private readonly AppDbContext _context;

        public IndexModel(ILogger<IndexModel> logger, UserManager<IdentityUser> userManager, AppDbContext context, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult OnGet()
        {
            if (_signInManager.IsSignedIn(User))
            {
                string userID = _userManager.GetUserId(User);
                User user = _context.Users.FirstOrDefault(p => p.UserAccountId == userID);
                if (user != null)
                {
                    return RedirectToPage("./HomePage", new { id = user.HomeDirId });
                }
                else
                {
                    return RedirectToPage("./CreateHomePage");
                }
            }
            else
            {
                return Page();
            }
        }
    }
}
