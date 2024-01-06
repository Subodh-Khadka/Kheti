using Kheti.Models;
using Microsoft.EntityFrameworkCore;

namespace Kheti.Data
{
    public class ApplicationDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Category> Categories { get; set; }

        public DbSet<KhetiApplicationUser> KhetiApplicationUsers { get; set; }
        public DbSet<Product> Products { get; set; }
    }
}
