// DataSeeder.cs
using Entities.Models;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Entities;

namespace OrderManagement.Seeding
{
    public static class DataSeeder
    {
        public static void SeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (!context.Orders.Any())
            {
                var order1 = new Order
                {
                    OrderId = Guid.NewGuid(),
                    OrderNumber = "ORD001",
                    CustomerName = "John Doe",
                    OrderDate = DateTime.Now,
                    TotalAmount = 66.5m
                };

                var order2 = new Order
                {
                    OrderId = Guid.NewGuid(),
                    OrderNumber = "ORD002",
                    CustomerName = "Jane Smith",
                    OrderDate = DateTime.Now,
                    TotalAmount = 225.8m
                };

                context.Orders.AddRange(order1, order2);
                context.SaveChanges();
            }
        }
    }
}
