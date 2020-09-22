using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SimpleCloudStorage.Models;

namespace SimpleCloudStorage.Pages
{
    public class PublicModel : PageModel
    {

        private readonly AppDbContext _context;
        private UserManager<IdentityUser> _userManager;
        private string _storageLocation;

        public PublicModel(AppDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _storageLocation = configuration["StorageLocation"];
        }

        public PublicFile PubFile { get; set; }
        public async Task<IActionResult> OnGetAsync(string id)
        {
            PubFile = await _context.PublicFiles.FirstOrDefaultAsync(p => p.PublicId == id);
            PubFile.FromUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == PubFile.FromUserId);
            PubFile.Fso = await _context.FileSystemObjects.FirstOrDefaultAsync(f => f.Id == PubFile.FsoId);
            
            return Page();
        }
        public async Task<IActionResult> OnPostAsync(int fsoId, int returnId)
        {
            User FromUser = await _context.Users.FirstOrDefaultAsync(p => p.UserAccountId == _userManager.GetUserId(User));
            FileSystemObject fso = await _context.FileSystemObjects.FirstOrDefaultAsync(f => f.Id == fsoId);
            PublicFile newPublicFile = new PublicFile();
            newPublicFile.FromUserId = FromUser.Id;
            newPublicFile.FsoId = fsoId;
            newPublicFile.SharedDate = DateTime.Now;
            newPublicFile.PublicId = genHashFileName(fso.Name);

            try
            {
                _context.PublicFiles.Add(newPublicFile);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Page();
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

        public string genHashFileName(string fileName)
        {
            StringBuilder sb = new StringBuilder();
            MemoryStream stream;

            using (SHA256 mySHA256 = SHA256.Create())
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(fileName + DateTime.Now.ToString());
                stream = new MemoryStream(byteArray);
                byte[] hashValue = mySHA256.ComputeHash(stream);
                for (int i = 0; i < hashValue.Length; i++) sb.Append($"{hashValue[i]:x2}");
            }
            return sb.ToString();
        }
    }
}