using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoginMicroservice.Models;

namespace LoginMicroservice.Contexts
{
    public class LoginContext : DbContext
    {
        public LoginContext(DbContextOptions<LoginContext> options) : base(options)
        { }
        public DbSet<User> Users { get; set; }
        public DbSet<Usertoken> Usertokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<Usertoken>().ToTable("Usertoken");
          
        }
             
    }
}

