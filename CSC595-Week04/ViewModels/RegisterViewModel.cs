using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using System.ComponentModel.DataAnnotations;

namespace CSC595_Week04.Views
{
    public class RegisterViewModel : LoginViewModel
    {
        [Required(ErrorMessage = "You must enter a first name!")]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "You must enter a last name!")]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "You must enter an email address!")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        
    }
}
