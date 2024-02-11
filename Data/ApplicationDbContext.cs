using Kheti.Models;
using Microsoft.EntityFrameworkCore;

namespace Kheti.Data
{
    public class ApplicationDbContext : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options){}       
        public DbSet<Category> Categories { get; set; }
        public DbSet<KhetiApplicationUser> KhetiApplicationUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<ProductComment> ProductComments {  get; set; }
        public DbSet<ProductReply> ProductReplies  { get; set; }
        public DbSet<QueryForm> QueryForms { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShoppingCart>()
                .HasOne(s => s.Product)
                .WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ShoppingCart>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Other configurations...
            modelBuilder.Entity<OrderItem>()
            .HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.NoAction); // Specify ON DELETE NO ACTION

            modelBuilder.Entity<Favorite>()
                .HasOne(s => s.Product).WithMany()
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ProductComment>()
               .HasOne(s => s.Product).WithMany()
               .HasForeignKey(s => s.ProductId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ProductReply>()
              .HasOne(s => s.ProductComment).WithMany()
              .HasForeignKey(s => s.ProductCommentId)
              .OnDelete(DeleteBehavior.NoAction);


            base.OnModelCreating(modelBuilder);
        }
    }


}
