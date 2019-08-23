using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Quack.Models.Account;

namespace Quack.Models
{
    public class QuackDbContext : IdentityDbContext<User, Role, int>
    {
        public DbSet<Post> Post { get; set; }
        public DbSet<BotModel> Bot { get; set; }

        public QuackDbContext(DbContextOptions<QuackDbContext> options)
            : base(options)
        {}
    }
}
