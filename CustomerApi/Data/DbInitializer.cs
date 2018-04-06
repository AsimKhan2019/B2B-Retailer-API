using System.Collections.Generic;
using System.Linq;
using CustomerApi.Models;
using System;

namespace CustomerApi.Data
{
    public static class DbInitializer
    {
        // This method will create and seed the database.
        public static void Initialize(CustomerApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            List<Customer> customers = new List<Customer>
            {
                new Customer { Id=1, BillingAddress = "Havnegade 63 B,  6700, Esbjerg", CompanyName = "EASV", Email = "trialemail@easv.dk", Phone ="1345 5684", ShippingAddress = "Havnegade 63 B,  6700, Esbjerg" }
            };



            context.Customers.AddRange(customers);
            context.SaveChanges();
        }
    }
}
