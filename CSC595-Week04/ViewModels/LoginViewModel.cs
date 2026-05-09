using System.ComponentModel.DataAnnotations;

namespace CSC595_Week04.Views
{
    public class LoginViewModel
    {
        [Display(Name = "User Name")]
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "remember me?")]
        public bool Remember { get; set; }
    }
}
