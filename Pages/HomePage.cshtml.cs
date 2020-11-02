using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Compression;
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
using Microsoft.Extensions.Configuration;
using SimpleCloudStorage.Models;

namespace SimpleCloudStorage.Pages
{
    [Authorize]
    public class HomePageModel : PageModel
    {
        private readonly AppDbContext _context;
        private UserManager<IdentityUser> _userManager;
        //private IWebHostEnvironment _webroot;
        private string _storageLocation;
        private long _premiumUserDiskSize;
        private long _normalUserDiskSize;

        public HomePageModel(AppDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webroot, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            //_webroot = webroot;
            _storageLocation = configuration["StorageLocation"];
            _premiumUserDiskSize = long.Parse(configuration["StorageSize:PremiumUser"]);
            _normalUserDiskSize = long.Parse(configuration["StorageSize:NormalUser"]);
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
        public IList<string> Roles { get; set; }

        [BindProperty]
        public long TotalBytes { get; set; }
        
        [BindProperty]
        public long CurrentUserDiskSize { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
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
                var folder = _storageLocation + CurrentUser.UserAccountId;
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

            var user = await _userManager.GetUserAsync(User);
            Roles = await _userManager.GetRolesAsync(user);

            return Page();
        }

        public async Task<JsonResult> OnGetUserData()
        {
            var aspUsr = await _userManager.GetUserAsync(User);
            var rolesList = await _userManager.GetRolesAsync(aspUsr);
            var user = await _context.Users.FirstOrDefaultAsync(p => p.UserAccountId == aspUsr.Id);
            var usedBytes = await GetFsoSize(user.HomeDirId);
            long diskSize = 0;
            if (rolesList.Contains("NormalUser"))
            {
                diskSize = _normalUserDiskSize;
            }
            else if (rolesList.Contains("PremiumUser"))
            {
                diskSize = _premiumUserDiskSize;
            }

            return new JsonResult(new UserInfo() { Name = user.Name, TotalBytes = diskSize, UsedBytes = usedBytes, Roles = rolesList.ToList()});
        }

        public async Task<IActionResult> OnPostCreateFolderAsync(int returnId, string fsoName)
        {
            if (fsoName != null) 
            {

                FileSystemObject newFso = new FileSystemObject();
                newFso.Name = fsoName;
                newFso.ParentId = returnId;
                newFso.IsFolder = true;
                newFso.CreateDate = DateTime.Now;

                if (!ModelState.IsValid)
                {
                    return Page();
                }
                try
                {
                    _context.FileSystemObjects.Add(newFso);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return RedirectToPage("HomePage", new { id = returnId });
                }
                
                return RedirectToPage("HomePage", new { id = returnId });
            }
            
                       
            return RedirectToPage("HomePage", new { id = returnId });
        }
        
        public async Task<IActionResult> OnPostUploadAsync(int returnId, IFormFile file)
        {
            if (file != null)
            {
                string filePath = _storageLocation + _userManager.GetUserId(User) + "/";
                var uploadFileName = Path.GetFileName(file.FileName);
                var hashFileName = CreateMD5(uploadFileName);
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
                try
                {
                    _context.FileSystemObjects.Add(newFso);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return RedirectToPage("./HomePage", new { id = returnId });
                }
            }
            return RedirectToPage("./HomePage", new { id = returnId });
        }
        public async Task<ActionResult> OnPostDownloadAsync(string fsoIdcsv, int dirId)
        {
            if (fsoIdcsv == null || fsoIdcsv == "") { return Page(); }
            string[] fsoIdArr = fsoIdcsv.Split(',');
            List<FileSystemObject> fsoList = new List<FileSystemObject>();
            foreach (var fsoId in fsoIdArr)
            {
                var fso = await _context.FileSystemObjects.FirstOrDefaultAsync(f => f.Id == int.Parse(fsoId));
                fsoList.Add(fso);
            }

            var ms = new MemoryStream();

            if (fsoList.Count == 1 && !fsoList[0].IsFolder)
            {
                string fullFilePath = _storageLocation + _userManager.GetUserId(User) + "/" + fsoList[0].FileName;
                using (var stream = new FileStream(fullFilePath, FileMode.Open))
                {
                    await stream.CopyToAsync(ms);
                }
                ms.Seek(0, SeekOrigin.Begin);
                return File(ms, "application/octet-stream", fsoList[0].Name);
            }
            else
            {
                ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Create, true);
                foreach (var fso in fsoList)
                {
                    await AddFsoToArchiveAsync(archive, fso, dirId);
                }

                archive.Dispose();
                ms.Seek(0, SeekOrigin.Begin);
                return File(ms, "application/zip", "files.zip");
            }
        }
        private async Task AddFsoToArchiveAsync(ZipArchive archive, FileSystemObject fso, int rootDirId)
        {
            string fsoPath = String.Empty;
            var parseObj = await _context.FileSystemObjects.FindAsync(fso.ParentId);
            while (parseObj.Id != rootDirId)
            {
                fsoPath = fsoPath.Insert(0, parseObj.Name + "/");
                parseObj.Parent = await _context.FileSystemObjects.FindAsync(parseObj.ParentId);
                parseObj = parseObj.Parent;
            }

            if (!fso.IsFolder)
            {
                string fullFilePath = _storageLocation + _userManager.GetUserId(User) + "/" + fso.FileName;
                archive.CreateEntryFromFile(fullFilePath, fsoPath+fso.Name, CompressionLevel.Optimal);
            }
            else
            {
                archive.CreateEntry(fsoPath + fso.Name+"/");
                foreach (var c in await GetFsoContentAsync(fso))
                {
                    await AddFsoToArchiveAsync(archive, c, rootDirId);
                }
            }
        }

        public async Task OnPostRenameAsync(int id, string newName)
        {
            if (newName != "" && newName != null)
            {
                var fso = await _context.FileSystemObjects.FirstOrDefaultAsync(f => f.Id == id);
                fso.Name = newName;
                _context.Update(fso);
                await _context.SaveChangesAsync();
            }  
        }

        private async Task<List<FileSystemObject>> GetFsoContentAsync(FileSystemObject fso)
        {
            List<FileSystemObject> FsoList = await _context.FileSystemObjects.ToListAsync();

            var folderContent = from f in FsoList
                                where f.ParentId == fso.Id
                                select f;
            return folderContent.ToList();
        }

        public async Task OnPostDeleteAsync(string fsoIdcsv)
        {
            if (fsoIdcsv == null || fsoIdcsv == "") { Page(); }
            string[] fsoIdArr = fsoIdcsv.Split(',');
            foreach (var fsoId in fsoIdArr)
            {
                await DeleteFsoAsync(int.Parse(fsoId));
            }
        }

        private async Task DeleteFsoAsync(int id)
        {
            var fso = await _context.FileSystemObjects.FirstOrDefaultAsync(f => f.Id == id);
            if (!fso.IsFolder)
            {
                var fullFilePath = _storageLocation + _userManager.GetUserId(User) + "/" + fso.FileName;
                _context.FileSystemObjects.Remove(fso);
                await _context.SaveChangesAsync();
                DeleteFile(fullFilePath);
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
                    await DeleteFsoAsync(f.Id);
                }
                _context.FileSystemObjects.Remove(fso);
                await _context.SaveChangesAsync();
            }
        }

        private void DeleteFile(string fullPath)
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

        private string CreateMD5(string input)
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

        private async Task<long> GetFsoSize(int id)
        {
            long bytesCount = 0;
            var fso = await _context.FileSystemObjects.FirstOrDefaultAsync(f => f.Id == id);
            if (!fso.IsFolder)
            {
                bytesCount = (long)fso.FileSize;
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
                    bytesCount += await GetFsoSize(f.Id);
                }
            }
            return bytesCount;
        }

        private class UserInfo
        {
            public string Name { get; set; }
            public long TotalBytes { get; set; }
            public long UsedBytes { get; set; }
            public List<string> Roles { get; set; }
        }

       
    }
}