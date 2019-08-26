using System;
using Microsoft.AspNetCore.Identity;

namespace Quack.Models
{
    public class User : IdentityUser<int>
    {
        public string firstName { get; set; }

        public string lastName { get; set; }

        public string avatarUrl { get; set; } = "/images/blank-profile.png";

        public bool deletable { get; set; } = true;
    }

    public class UserDTO
    {
        public UserDTO() {
            ID = -1;
            username = "unknown";
            avatarUrl = "/images/blank-profile.png";
        }
        public UserDTO(User user) {
            ID = user.Id;
            username = user.UserName;
            avatarUrl = user.avatarUrl;
        }

        public int ID { get; set; }

        public string username { get; set; }

        public string avatarUrl { get; set; }

        public int postsCount { get; set; }

        public int commentsCount { get; set; }

        public bool followed { get; set; }
 
        public int followingCount { get; set; }

        public int followedByCount { get; set; }
    }


}
