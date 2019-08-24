using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace Quack.Models
{
    public class Comment
    {
        [Key]
        public int ID { get; set; }

        public int authorID { get; set; }
        [ForeignKey("authorID")]
        public virtual User author { get; set; }

        public string text { get; set; }

        public DateTime datePublished { get; set; }

        public int postID { get; set; }
        [ForeignKey("postID")]
        public virtual Post post { get; set; }
    }

    public class CommentDTO
    {
        public CommentDTO(Comment comment) {
            ID = comment.ID;
            authorID = comment.authorID;
            username = comment.author.UserName;
            avatarUrl = comment.author.avatarUrl;
            text = comment.text;
            datePublished = comment.datePublished;
            postID = comment.postID;
        }

        public int ID { get; set; }

        public int postID { get; set; }

        public int authorID { get; set; }

        public string avatarUrl { get; set; }

        public string username { get; set; }

        public string text { get; set; }

        public DateTime datePublished { get; set; }
    }

}
