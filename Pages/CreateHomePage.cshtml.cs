using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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
        
        [BindProperty]
        public bool UserExists { get; set; } 

        public CreateHomePageModel(UserManager<IdentityUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            UserExists = false;
            var aspUserId = _userManager.GetUserId(User);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserAccountId == aspUserId);
            if (user != null) {
                UserExists = true;
                Name = user.Name;
                var fso = await _context.FileSystemObjects.FirstOrDefaultAsync(f => f.Id == user.HomeDirId);
                if (fso != null)
                {
                    HomeDirName = fso.Name;
                }
            }
            return Page();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            var aspUserId = _userManager.GetUserId(User);

            if (aspUserId != null)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserAccountId == aspUserId);
                if (user != null)
                {
                    var fso = await _context.FileSystemObjects.FirstOrDefaultAsync(f => f.Id == user.HomeDirId);
                    fso.Name = HomeDirName;
                    user.Name = Name;
                    _context.Update(fso);
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    FileSystemObject newHomeDir = new FileSystemObject();
                    newHomeDir.IsFolder = true;
                    newHomeDir.Name = HomeDirName;
                    newHomeDir.ParentId = null;
                    newHomeDir.CreateDate = DateTime.Now;
                    _context.FileSystemObjects.Add(newHomeDir);
                    await _context.SaveChangesAsync();

                    User newUser = new User();
                    newUser.Name = Name;
                    newUser.UserAccountId = aspUserId;
                    newUser.HomeDir = newHomeDir;
                    newUser.HomeDirId = newHomeDir.Id;
                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    return RedirectToPage("HomePage", new { id = newHomeDir.Id });
                }
            }
            return RedirectToPage("./Index");
        }
    }
}