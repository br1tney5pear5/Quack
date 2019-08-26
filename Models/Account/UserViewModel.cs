using System;
using System.ComponentModel.DataAnnotations;

namespace Quack.Models.Account{
    public class UserViewModel {
        public UserDTO userDTO { get; set; }
        public UserDTO currentUserDTO { get; set; }
    }
}
