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
                new Product { Name = "Hammer", Price = 100, ItemsInStock = 10 },
                new Product { Name = "Screwdriver", Price = 70, ItemsInStock = 20 },
                new Product { Name = "Drill", Price = 500, ItemsInStock = 2 }
            };

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
