using Microsoft.EntityFrameworkCore;
using BlogAppApi.Models;

namespace BlogAppApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Blog> Blogs { get; set; }
    }
}