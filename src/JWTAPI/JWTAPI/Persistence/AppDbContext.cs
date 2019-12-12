using System;
using System.Collections.Generic;
using JWTAPI.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JWTAPI.Persistence
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        private IPasswordHasher<User> _passwordHasher { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options, IPasswordHasher<User> passwordHasher) : base(options)
        {
            _passwordHasher = passwordHasher;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });
        }
    }
}