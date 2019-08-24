using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


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

        public DateTime datePublished { get; set; }

        public virtual ICollection<Comment> comments { get; set; }
    }

    public class PostDTO
    {
        public PostDTO(Post post) {
          ID = post.ID;
          authorID = post.authorID;
          username = post.author.UserName;
          avatarUrl = post.author.avatarUrl;
          content = post.content;
          datePublished = post.datePublished;
          comments = post.comments
              .AsQueryable()
              .Select(c => new CommentDTO(c))
              .ToList();
        }


        public int ID { get; set; }

        public int authorID { get; set; }

        public string username { get; set; }

        public string avatarUrl { get; set; }

        public virtual PostContent content { get; set; }

        public DateTime datePublished { get; set; }

        public ICollection<CommentDTO> comments { get; set; }
    }
}
