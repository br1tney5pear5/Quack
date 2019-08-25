using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Quack.Models.Account;

namespace Quack.Models
{
    public class QuackDbContext : IdentityDbContext<User, Role, int>
    {
        public DbSet<Post> Post { get; set; }
        public DbSet<PostContent> PostContent { get; set; }
        public DbSet<Tag> Tag { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<BotModel> Bot { get; set; }
        public DbSet<Following> Following { get; set; }

        public QuackDbContext(DbContextOptions<QuackDbContext> options)
            : base(options)
        {}
    }
}
