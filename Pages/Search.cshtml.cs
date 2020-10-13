using System;
using System.Collections.Generic;
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
    public class SearchModel : PageModel
    {
        private readonly AppDbContext _context;
        private UserManager<IdentityUser> _userManager;
        private string _storageLocation;

        public IEnumerable<FileSystemObject> SearchResults { get; set; }
        
        public List<FileSystemObject> SearchScope { get; set; }
        
        [BindProperty]
        public FileSystemObject CurrentDir { get; set; }

        public SearchModel(AppDbContext context, UserManager<IdentityUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _storageLocation = configuration["StorageLocation"];
            SearchResults = new List<FileSystemObject>();
            SearchScope = new List<FileSystemObject>();
        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string keyword, int dirId)
        {
            CurrentDir = _context.FileSystemObjects.FirstOrDefault(d => d.Id == dirId);
            if (CurrentDir != null)
            {
                ViewData["ReturnId"] = CurrentDir.Id;
                if (keyword != null)
                {
                    await searchDir(dirId);
                    filterResults(keyword);
                }
                return Page();
            }
            else
            {
                return RedirectToPage("./Index");
            }
        }

        public async Task searchDir(int id)
        {
            List<FileSystemObject> fsoList = await _context.FileSystemObjects.ToListAsync();
            IEnumerable<FileSystemObject> subDirList = new List<FileSystemObject>();
            subDirList = from dir in fsoList
                         where dir.ParentId == id
                         select dir;
            foreach (var f in subDirList)
            {
                SearchScope.Add(f);
                if (f.IsFolder)
                {
                    await searchDir(f.Id);
                }
            }
        }

        public void filterResults(string kWord)
        {
            SearchResults = from f in SearchScope
                            where f.Name.ToUpper().Contains(kWord.ToUpper())
                            orderby f.IsFolder descending, f.Name ascending
                            select f;
        }
    }
}