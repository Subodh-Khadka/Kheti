using Kheti.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;

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
        public DbSet<ExpertProfile> ExpertProfiles { get; set; }
        public DbSet<QueryComment> QueryComments { get; set; }
        public DbSet<QueryReply> QueryReplies { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<RentalEquipment> RentalEquipment { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingComments> BookingComments { get; set; }


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
            .OnDelete(DeleteBehavior.NoAction);

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

            modelBuilder.Entity<QueryComment>()
              .HasOne(s => s.Form).WithMany()              
              .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<QueryReply>()
              .HasOne(s => s.Comment).WithMany()
              .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
            .HasOne(s => s.Product).WithMany()
            .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<Review>()
            .HasOne(s => s.Product).WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Review>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
               .HasOne(s => s.User)
               .WithMany()
               .HasForeignKey(s => s.UserId)
               .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Booking>()
          .HasOne(s => s.Product).WithMany()
          .HasForeignKey(s => s.ProductId)
          .OnDelete(DeleteBehavior.NoAction);

            base.OnModelCreating(modelBuilder);
        }
    }


}
