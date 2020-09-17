using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleCloudStorage.Models;

namespace SimpleCloudStorage.Pages
{
    [Authorize]
    public class HomePageModel : PageModel
    {
        private readonly AppDbContext _context;
        private UserManager<IdentityUser> _userManager;
        private IWebHostEnvironment _webroot;

        public HomePageModel(AppDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webroot)
        {
            _context = context;
            _userManager = userManager;
            _webroot = webroot;
        }

        [BindProperty]
        public User CurrentUser { get; set; }

        [BindProperty]
        public FileSystemObject CurrentDir { get; set; }

        [BindProperty]
        public List<FileSystemObject> fullPathList { get; set; }

        [BindProperty]
        public IEnumerable<FileSystemObject> Children { get; set; }
        
        public async Task<IActionResult> OnGetAsync(int id)
        {
            CurrentDir = await _context.FileSystemObjects.FindAsync(id);
            if (CurrentDir == null)
            {
                return RedirectToPage("./Index");
            }

            CurrentUser = _context.Users.FirstOrDefault(p => p.UserAccountId == _userManager.GetUserId(User));
            //Creating storage folder on server
            if (CurrentUser != null)
            {
                var folder = _webroot.WebRootPath + "\\storage\\" + CurrentUser.UserAccountId;
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
            FileSystemObject parseObj = CurrentDir;
            fullPathList = new List<FileSystemObject>();
            while (parseObj.ParentId != null)
            {
                fullPathList.Insert(0, parseObj);
                parseObj.Parent = await _context.FileSystemObjects.FindAsync(parseObj.ParentId);
                parseObj = parseObj.Parent;
            }
            fullPathList.Insert(0, parseObj);
            //parseObj points to homedir now
            if (parseObj.Id != CurrentUser.HomeDirId)
            {
                return RedirectToPage("./HomePage", new { id = CurrentUser.HomeDirId });
            }

            List<FileSystemObject> FsoList = await _context.FileSystemObjects.ToListAsync();

            Children = from dir in FsoList
                       where dir.ParentId == CurrentDir.Id
                       orderby dir.IsFolder descending, dir.Name ascending
                       select dir;


            
            /*IDictionary<string, string> FilesUploadDates = new Dictionary<string, string>();
            IDictionary<string, string> FilesSizes = new Dictionary<string, string>();
            foreach (var c in Children)
            {
                if (!c.IsFolder)
                {
                    Models.File f = await _context.Files.FirstOrDefaultAsync(f => f.FsoId == c.Id);
                    var fName = c.Name;
                    var fSize = f.FileSize;
                    var fDate = f.UploadDate;
                    
                    FilesSizes.Add(c.Name, BytesToString(fSize));
                    FilesUploadDates.Add(c.Name, f.UploadDate.ToString("MM/dd/yyyy h:mm tt"));
                }
            }
            ViewData["FilesSizes"] = FilesSizes;
            ViewData["FilesUploadDates"] = FilesUploadDates;*/
            return Page();
        }

        public async Task<IActionResult> OnPostCreateFolderAsync(int parentId, string fsoName)
        {
            if (fsoName != null) 
            {
                FileSystemObject NewFso = new FileSystemObject();
                NewFso.Name = fsoName;
                NewFso.ParentId = parentId;
                NewFso.IsFolder = true;

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                _context.FileSystemObjects.Add(NewFso);
                await _context.SaveChangesAsync();
                return RedirectToPage("HomePage", new { id = NewFso.Id });
            }
            
                       
            return RedirectToPage("HomePage", new { id = parentId });
        }

        public async Task<IActionResult> OnPostUploadAsync(int returnId, IFormFile file)
        {
            if (file != null)
            {
                string filePath = _webroot.WebRootPath + "\\storage\\" + _userManager.GetUserId(User) + "\\";
                var uploadFileName = Path.GetFileName(file.FileName);
                var hashFileName = genHashFileName(uploadFileName);
                var fileSize = file.Length;

                using (var stream = System.IO.File.Create(filePath + hashFileName))
                {
                    await file.CopyToAsync(stream);
                }

                FileSystemObject newFso = new FileSystemObject();
                newFso.Name = uploadFileName;
                newFso.ParentId = returnId;
                newFso.IsFolder = false;
                newFso.FileName = hashFileName;
                newFso.FileSize = file.Length;
                newFso.CreateDate = DateTime.Now;
                _context.FileSystemObjects.Add(newFso);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("./HomePage", new { id = returnId });
        }

        public async Task<ActionResult> OnPostDownloadAsync(int id, string name)
        {
            string filePath = _webroot.WebRootPath + "\\storage\\" + _userManager.GetUserId(User) + "\\";
            var hashFileName = _context.FileSystemObjects.FirstOrDefault(f => f.Id == id).FileName;

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath + hashFileName, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", name);
        }

        public async Task<ActionResult> OnPostDeleteAsync(int fsoId, int returnId)
        {
            await deleteFsoAsync(fsoId);
            return RedirectToPage("./HomePage", new { id = returnId });

        }

        private async Task deleteFsoAsync(int id)
        {
            var fso = await _context.FileSystemObjects.FirstOrDefaultAsync(f => f.Id == id);
            if (!fso.IsFolder)
            {
                var fullFilePath = _webroot.WebRootPath + "\\storage\\" + _userManager.GetUserId(User) + "\\" + fso.FileName;
                _context.FileSystemObjects.Remove(fso);
                await _context.SaveChangesAsync();
                deleteFile(fullFilePath);
            }
            else
            {
                List<FileSystemObject> fsoList = await _context.FileSystemObjects.ToListAsync();
                IEnumerable<FileSystemObject> subDirList = new List<FileSystemObject>();
                subDirList = from dir in fsoList
                           where dir.ParentId == id
                           select dir;
                foreach (var f in subDirList)
                {
                    await deleteFsoAsync(f.Id);
                }
                _context.FileSystemObjects.Remove(fso);
                await _context.SaveChangesAsync();
            }
        }

        private void deleteFile(string fullPath)
        {
            if (System.IO.File.Exists(fullPath))
            {
                try
                {
                    System.IO.File.Delete(fullPath);
                }
                catch (Exception ex)
                {
                }
            }
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
        public string bytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
    }
}