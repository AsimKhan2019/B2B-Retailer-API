using System.Collections.Generic;
using System.Linq;
using ProductApi.Models;

namespace ProductApi.Data
{
    public static class DbInitializer
    {
        // This method will create and seed the database.
        public static void Initialize(ProductApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Products.Any())
            {
                return;   // DB has been seeded
            }

            List<Product> products = new List<Product>
            {
                new Product { Name = "Hammer", Price = 100, ItemsInStock = 10 ,Category= "Hand Tools", Description = "Get all of those tough jobs done with the help of this hammer. It is built to be durable while taking on tasks that are hard to do."},
                new Product { Name = "Screwdriver", Price = 70, ItemsInStock = 20,Category = "Hand Tools", Description = "A handy tool to keep on hand in the workshop, around the house and elsewhere. The cushioned, ergonomic handle is comfortable to grip while you tighten or loosen. " },
                new Product { Name = "Drill", Price = 500, ItemsInStock = 2, Category = "Tools", Description = "It gives you the tools you need to complete a variety of home repair projects. The motorized drill runs on the included rechargeable lithium-ion batteries. " }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
