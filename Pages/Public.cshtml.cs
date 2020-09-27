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
        [BindProperty]
        public PublicFile PubFile { get; set; }
        public async Task<IActionResult> OnGetAsync(string ?id)
        {
            if (id != null)
            {
                PubFile = await _context.PublicFiles.FirstOrDefaultAsync(p => p.PublicId == id);
                PubFile.FromUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == PubFile.FromUserId);
                PubFile.Fso = await _context.FileSystemObjects.FirstOrDefaultAsync(f => f.Id == PubFile.FsoId);
                return Page();
            }
            else
            {
                return RedirectToPage("Index");
            }
            
        }
        public async Task<IActionResult> OnPostAsync(int fsoId, int returnId)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            User FromUser = await _context.Users.FirstOrDefaultAsync(p => p.UserAccountId == _userManager.GetUserId(User));
            FileSystemObject fso = await _context.FileSystemObjects.FirstOrDefaultAsync(f => f.Id == fsoId);
            PublicFile newPublicFile = new PublicFile();
            newPublicFile.FromUserId = FromUser.Id;
            newPublicFile.FsoId = fsoId;
            newPublicFile.SharedDate = DateTime.Now;
            newPublicFile.PublicId = CreateMD5(fso.Name);

            try
            {
                _context.PublicFiles.Add(newPublicFile);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return RedirectToPage("HomePage", new { id = returnId });
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

/*        public string GenHashFileName(string fileName)
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
        }*/
        public string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input + DateTime.Now);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                sb.Insert(8, "-")
                    .Insert(13, "-")
                    .Insert(18, "-")
                    .Insert(23, "-");

                return sb.ToString();
            }
        }
    }
}