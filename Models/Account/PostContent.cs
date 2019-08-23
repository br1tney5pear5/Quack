using System;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Quack.Models
{
    public class PostContent
    {
        public int ID { get; set; }
        public string text { get; set; }
        public virtual ICollection<Tag> tags { get; set; }
    }
}
