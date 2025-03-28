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
                    OrderId = Guid.Parse("F4816224-70D6-4491-AC52-34F298ACE16F"), 
                    OrderNumber = "ORD001",
                    CustomerName = "John Doe",
                    OrderDate = DateTime.Now,
                    TotalAmount = 66.5m
                };

                var order2 = new Order
                {
                    OrderId = Guid.Parse("735886C0-FAF3-49CA-9776-8A20B756F1CB"), 
                    OrderNumber = "ORD002",
                    CustomerName = "Jane Smith",
                    OrderDate = DateTime.Now,
                    TotalAmount = 225.8m
                };

                var orderItem1 = new OrderItem
                {
                    OrderItemId = Guid.Parse("D20882DF-7FCA-4EE8-88BB-37D2FC75E63F"),
                    OrderId = order1.OrderId, 
                    ProductName = "Product A",
                    Quantity = 2,
                    UnitPrice = 10.00m,
                    TotalPrice = 20.00m
                };

                var orderItem2 = new OrderItem
                {
                    OrderItemId = Guid.Parse("2E27B6A4-469D-4D7F-8B8B-54AF129675FD"),
                    OrderId = order1.OrderId, 
                    ProductName = "Product B",
                    Quantity = 3,
                    UnitPrice = 15.50m,
                    TotalPrice = 46.50m
                };

                var orderItem3 = new OrderItem
                {
                    OrderItemId = Guid.Parse("24D71AC2-0A9C-4914-9FD3-13BC25D42694"),
                    OrderId = order2.OrderId, 
                    ProductName = "Product C",
                    Quantity = 7,
                    UnitPrice = 25.40m,
                    TotalPrice = 177.8m
                };

                var orderItem4 = new OrderItem
                {
                    OrderItemId = Guid.Parse("AC90B8BC-349D-43FD-87A6-6A7ED8057697"),
                    OrderId = order2.OrderId, 
                    ProductName = "Product D",
                    Quantity = 4,
                    UnitPrice = 12.00m,
                    TotalPrice = 48.00m
                };
           

                context.Orders.AddRange(order1, order2);
                context.OrderItems.AddRange(orderItem1,orderItem2,orderItem3,orderItem4);
                context.SaveChanges();
            }
        }
    }
}
