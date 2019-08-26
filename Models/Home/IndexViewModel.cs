using System;
using System.ComponentModel.DataAnnotations;

namespace Quack.Models.Home{
    public class IndexViewModel {
        public PostContent postContent { get; set; }
        public UserDTO userDTO { get; set; }
    }
}
