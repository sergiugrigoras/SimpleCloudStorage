using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleCloudStorage.Models;

namespace SimpleCloudStorage.Pages
{
    public class RegisterModel : PageModel
    {
        private UserManager<IdentityUser> _userManager;
        private SignInManager<IdentityUser> _signInManager;

        [Required, DataType(DataType.EmailAddress)]
        [BindProperty]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        [BindProperty]
        public string Password { get; set; }

        [DataType(DataType.Password), Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }

        [BindProperty]
        public string AccountType { get; set; }
        public RegisterModel(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult OnGet()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToPage("../Index");
            }
            return Page();
        }

        public IActionResult OnPost()
        {
            IdentityUser newAspNetUser = new IdentityUser();
            newAspNetUser.UserName = Email;
            newAspNetUser.Email = Email;

            IdentityResult result = _userManager.CreateAsync(newAspNetUser, Password).Result;
            if (result.Succeeded)
            {
                _userManager.AddToRoleAsync(newAspNetUser, AccountType).Wait();
                _signInManager.SignInAsync(newAspNetUser, false).Wait();
                return RedirectToPage("../CreateHomePage");
            }
            return RedirectToPage("../Index");
        }

    }
}



/*
 
 */
