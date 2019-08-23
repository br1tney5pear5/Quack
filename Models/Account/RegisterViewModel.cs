using System;
using System.ComponentModel.DataAnnotations;

namespace Quack.Models.Account{
    public class RegisterViewModel {

        [Required]
        public string username { get; set; }

        [Required]
        [EmailAddress]
        public string email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("password")]
        [Display(Name = "Confirm Password")]
        public string confirmPassword { get; set; }
    }
}
