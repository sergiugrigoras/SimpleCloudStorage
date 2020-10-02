using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SimpleCloudStorage.Models;

namespace SimpleCloudStorage.Pages
{
    public class NoteModel : PageModel
    {
        private readonly AppDbContext _context;
        private UserManager<IdentityUser> _userManager;

        [BindProperty]
        public IEnumerable<Note> Notes { get; set; }
        
        [BindProperty]
        public Note Note { get; set; }


        public NoteModel(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = _context.Users.FirstOrDefault(p => p.UserAccountId == _userManager.GetUserId(User));
            if (user != null)
            {
                List<Note> NotesList = await _context.Notes.ToListAsync();

                Notes = from n in NotesList
                        where n.UserId == user.Id
                        orderby n.CreationDate descending
                        select n;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

                var user = _context.Users.FirstOrDefault(p => p.UserAccountId == _userManager.GetUserId(User));
                Note.UserId = user.Id;
                Note.CreationDate = DateTime.Now;
                _context.Notes.Add(Note);
                await _context.SaveChangesAsync();
                return RedirectToPage("Note");
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {
            var user = _context.Users.FirstOrDefault(p => p.UserAccountId == _userManager.GetUserId(User));
            Note.UserId = user.Id;
            Note.CreationDate = DateTime.Now;
            _context.Notes.Update(Note);
            await _context.SaveChangesAsync();
            return RedirectToPage("Note");
        }
        public async Task<IActionResult> OnPostDeleteAsync(int noteId)
        {
            var note = _context.Notes.Find(noteId);
            if (note != null)
            {
                _context.Notes.Remove(note);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("Note");
        }

    }
}