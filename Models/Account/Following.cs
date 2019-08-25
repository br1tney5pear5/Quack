using System;
using Microsoft.AspNetCore.Identity;
using Quack.Models.Account;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Quack.Models
{
    public class Following
    {
        [Key]
        public int ID { get; set; }

        public int followerID { get; set; }
        [ForeignKey("followerID")]
        public virtual User follower { get; set; }

        public int followedID { get; set; }
        [ForeignKey("followedID")]
        public virtual User followed { get; set; }
    }
}
