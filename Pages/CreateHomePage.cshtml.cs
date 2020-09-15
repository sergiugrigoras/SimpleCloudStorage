using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleCloudStorage.Models;

namespace SimpleCloudStorage.Pages
{
    [Authorize]
    public class CreateHomePageModel : PageModel
    {

        private UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;

        [Required]
        [BindProperty]
        public string Name { get; set; }

        [Required]
        [BindProperty]
        public string HomeDirName { get; set; }

        public CreateHomePageModel(UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public void OnGet()
        {
            Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId != null)
            {
                FileSystemObject newHomeDir = new FileSystemObject();
                newHomeDir.IsFolder = true;
                newHomeDir.Name = HomeDirName;
                newHomeDir.ParentId = null;
                _context.FileSystemObjects.Add(newHomeDir);
                await _context.SaveChangesAsync();


                User newUser = new User();
                newUser.Name = Name;
                newUser.UserAccountId = currentUserId;
                newUser.HomeDir = newHomeDir;
                newUser.HomeDirId = newHomeDir.Id;
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return RedirectToPage("HomePage", new { id = newHomeDir.Id });
            }

            return RedirectToPage("./Index");

        }
    }
}