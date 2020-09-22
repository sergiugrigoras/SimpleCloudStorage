using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SimpleCloudStorage.Models;

namespace SimpleCloudStorage.Pages
{
    [Authorize]
    public class ShareModel : PageModel
    {
        private readonly AppDbContext _context;
        private UserManager<IdentityUser> _userManager;
        private string _storageLocation;

        public ShareModel(AppDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _storageLocation = configuration["StorageLocation"];
        }

        [BindProperty]
        public string Data { get; set; }

        [BindProperty]
        public IEnumerable<Share> SharedToUserList { get; set; }
        public IEnumerable<Share> SharedFromUserList { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            User user = await _context.Users.FirstOrDefaultAsync(p => p.UserAccountId == _userManager.GetUserId(User));
            SharedToUserList = new List<Share>();

            var shares = await _context.Shares.ToListAsync();

            SharedToUserList = from shr in shares
                               where shr.ToUserId ==user.Id
                               orderby shr.SharedDate descending
                               select shr;
            foreach (var s in SharedToUserList)
            {
                s.Fso = await _context.FileSystemObjects.FindAsync(s.FsoId);
                s.FromUser = await _context.Users.FindAsync(s.FromUserId);
                s.ToUser = await _context.Users.FindAsync(s.ToUserId);
            }


/*            foreach (var s in toUserShares)
            {
                fso = await _context.FileSystemObjects.FindAsync(s.FsoId);
                SharedFsoList.Add(fso);
            }*/
            return Page();
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
                    //Data = ex.Message;
                    return Page();
                }
                
                               
            }
            return RedirectToPage("HomePage", new { id = returnId });
        }

        public async Task<ActionResult> OnPostDownloadAsync(int fsoId, string fileName, int fromUserId)
        {
            User fromUser = await _context.Users.FindAsync(fromUserId);
            string filePath = _storageLocation + fromUser.UserAccountId + "/";
            var hashFileName = _context.FileSystemObjects.FirstOrDefault(f => f.Id == fsoId).FileName;

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath + hashFileName, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", fileName);
        }
    }
}