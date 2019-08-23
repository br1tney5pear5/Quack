using System;
using Microsoft.AspNetCore.Identity;

namespace Quack.Models
{
    public class User : IdentityUser<int>
    {
        public string firstName { get; set; }

        public string lastName { get; set; }
    }
}
