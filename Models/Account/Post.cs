using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace Quack.Models
{
    public class Post
    {
        [Key]
        public int ID { get; set; }

        public int authorID { get; set; }
        [ForeignKey("authorID")]
        public virtual User author { get; set; }

        public int contentID { get; set; }
        [ForeignKey("contentID")]
        public virtual PostContent content { get; set; }
    }
}
