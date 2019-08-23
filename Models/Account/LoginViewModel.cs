using System;
using System.ComponentModel.DataAnnotations;

namespace Quack.Models.Account{
    public class LoginViewModel {
        [Required]
        [Display(Name = "Username")]
        public string username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string password { get; set; }

        [Display(Name = "Remember me?")]
        public bool rememberMe { get; set; }
    }
}
