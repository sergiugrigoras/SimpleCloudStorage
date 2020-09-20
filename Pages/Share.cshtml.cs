using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleCloudStorage.Models;

namespace SimpleCloudStorage.Pages
{
    public class ShareModel : PageModel
    {
        private readonly AppDbContext _context;
        private UserManager<IdentityUser> _userManager;

        public ShareModel(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public string Data { get; set; }

        public void OnGet()
        {
            Page();
        }

        public async Task<IActionResult> OnPostAsync(string recipient, int fsoId, int returnId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var aspUsr = await _userManager.FindByEmailAsync(recipient);

            if (aspUsr != null)
            {
                User FromUser = _context.Users.FirstOrDefault(p => p.UserAccountId == _userManager.GetUserId(User));
                User ToUser = _context.Users.FirstOrDefault(p => p.UserAccountId == aspUsr.Id);

                Share newShare = new Share();
                newShare.FromUserId = FromUser.Id;
                newShare.ToUserId = ToUser.Id;
                newShare.FsoId = fsoId;
                newShare.SharedDate = DateTime.Now;

                try
                {
                    _context.Shares.Add(newShare);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    Data = ex.Message;
                    return Page();
                }
                
                               
            }
            return RedirectToPage("HomePage", new { id = returnId });
        }
    }
}