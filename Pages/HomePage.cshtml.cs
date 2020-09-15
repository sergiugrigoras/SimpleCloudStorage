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
        
        [BindProperty]
        public IDictionary<string, string> FilesSizes { get; set; }

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

            List<FileSystemObject> Directories = await _context.FileSystemObjects.ToListAsync();

            Children = from dir in Directories
                       where dir.ParentId == CurrentDir.Id
                       orderby dir.IsFolder descending, dir.Name ascending
                       select dir;

            FilesSizes = new Dictionary<string, string>();
            foreach (var c in Children)
            {
                if (!c.IsFolder)
                {
                    Models.File f = await _context.Files.FirstOrDefaultAsync(f => f.FsoId == c.Id);
                    var fName = c.Name;
                    var fSize = f.FileSize;
                    FilesSizes.Add(c.Name, BytesToString(fSize));
                }
            }
            ViewData["FilesSizes"] = FilesSizes;
            return Page();
        }

        public async Task<IActionResult> OnPostCreateFolderAsync(int parentId, string fsoName)
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

        public async Task<IActionResult> OnPostUploadAsync(int returnId, IFormFile file)
        {
            if (file!=null)
            {
                string filePath = _webroot.WebRootPath + "\\storage\\" + _userManager.GetUserId(User) + "\\";
                var fileName = Path.GetFileName(file.FileName);
                var hashFileName = genHashFileName(fileName);
                var fileSize = file.Length;

                using (var stream = System.IO.File.Create(filePath + hashFileName))
                {
                    await file.CopyToAsync(stream);
                }

                FileSystemObject newFso = new FileSystemObject();
                newFso.Name = fileName;
                newFso.ParentId = returnId;
                newFso.IsFolder = false;
                _context.FileSystemObjects.Add(newFso);
                await _context.SaveChangesAsync();

                Models.File newFile = new Models.File();
                newFile.FsoId = newFso.Id;
                newFile.Fso = newFso;
                newFile.FileName = hashFileName;
                newFile.FileSize = fileSize;
                _context.Files.Add(newFile);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("./HomePage", new { id = returnId });
        }

        public async Task<ActionResult> OnPostDownloadAsync(int id, string name)
        {
            string filePath = _webroot.WebRootPath + "\\storage\\" + _userManager.GetUserId(User) + "\\";
            var hashFileName = _context.Files.FirstOrDefault(f=> f.FsoId == id).FileName;

            var memory = new MemoryStream();
            using (var stream = new FileStream(filePath+hashFileName, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", name);
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
        public string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }
    }
}

//return File(fs, System.Web.MimeMapping.GetMimeMapping(fileName), fileName);
//"application/octet-stream"

/*
var path = _webroot.WebRootPath + "\\storage\\id.pdf";
var memory = new MemoryStream();

            using (var stream = new FileStream(path, FileMode.Open))
            {
                await stream.CopyToAsync(memory);
            }
            memory.Position = 0;

            return File(memory, "application/octet-stream", name);*/