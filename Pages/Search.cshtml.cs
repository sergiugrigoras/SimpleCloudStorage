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

        [BindProperty]
        public IEnumerable<FileSystemObject> SearchResults { get; set; }
        
        [BindProperty]
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
            /*SearchKeyword = keyword;
            DirId = dirId;*/
            CurrentDir = await _context.FileSystemObjects.FindAsync(dirId);

            if (keyword != null)
            {
                await searchDir(dirId);
                filterResults(keyword);
            }
            
            
            return Page();

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
            /*
             var results = from c in db.costumers
              where SqlMethods.Like(c.FullName, "%"+FirstName+"%,"+LastName)
              select c;
            */
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