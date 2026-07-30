using System;
using System.Collections.Generic;
using System.Linq;

//  Two related collections sharing a foreign key (CustomerId)----------------

List<Customer> customers = new List<Customer>
{
    new Customer(1, "Shahd"),
    new Customer(2, "Ali"),
    new Customer(3, "Sara"),
    new Customer(4, "Omar"),
    new Customer(5, "Layla"),
    new Customer(6, "Nour")
};

List<Order> orders = new List<Order>
{
    new Order(101, 1, new List<string> { "Laptop", "Mouse" }),
    new Order(102, 1, new List<string> { "Keyboard" }),
    new Order(103, 2, new List<string> { "Monitor", "HDMI Cable" }),
    new Order(104, 3, new List<string> { "Phone Case" }),
    new Order(105, 4, new List<string> { "Headphones", "Charger" }),
    new Order(106, 5, new List<string> { "Desk", "Chair", "Lamp" })
};


foreach (var order in orders)
{
    order.Amount = order.Items.Count * 50;
}

// 2. GroupBy: total order amount per customer------------------------
Console.WriteLine("----- Total Amount per Customer (GroupBy) -----");

var totalByCustomer = orders
    .GroupBy(o => o.CustomerId)
    .Select(g => new { CustomerId = g.Key, Total = g.Sum(o => o.Amount) });

foreach (var group in totalByCustomer)
{
    Console.WriteLine($"Customer {group.CustomerId}: Total = {group.Total}");
}

// 3. Join: customer names combined with their order amounts-----------------
Console.WriteLine("\n----- Customer Names + Order Amounts (Join) -----");

var customerOrderDetails = customers
    .Join(orders,
          c => c.Id,
          o => o.CustomerId,
          (c, o) => new { c.Name, o.OrderId, o.Amount });

foreach (var item in customerOrderDetails)
{
    Console.WriteLine($"{item.Name} - Order #{item.OrderId} - Amount: {item.Amount}");
}

// 4. SelectMany: flatten every item across every order----------------------------
Console.WriteLine("\n----- All Items Across All Orders (SelectMany) -----");

var allItems = orders.SelectMany(o => o.Items);

foreach (var item in allItems)
{
    Console.WriteLine($"- {item}");
}

//5. Demonstrating Deferred Execution ---------------------------------------
Console.WriteLine("\n----- Deferred Execution Demo -----");


var highValueOrdersQuery = orders.Where(o => o.Amount >= 100);


orders.Add(new Order(107, 6, new List<string> { "Bookshelf", "Desk Lamp", "Rug" })); 
orders[6].Amount = orders[6].Items.Count * 50;

Console.WriteLine("Query defined BEFORE the new order was added, but enumerated AFTER:");
foreach (var order in highValueOrdersQuery)
{
    Console.WriteLine($"- Order #{order.OrderId} (Amount: {order.Amount})");
}

Console.WriteLine("\nExplanation: The new order (#107, Amount 150) appears in the results");
Console.WriteLine("even though it was added AFTER the query was defined. This is because");
Console.WriteLine("LINQ's Where() uses deferred execution — the query is just a plan that");
Console.WriteLine("runs on the CURRENT state of 'orders' at the moment it's enumerated (the");
Console.WriteLine("foreach loop), not at the moment it was written.");


// Type Declarations--------------------------------------------------------------- 

public class Customer
{
    public int Id { get; }
    public string Name { get; }

    public Customer(int id, string name)
    {
        Id = id;
        Name = name;
    }
}

public class Order
{
    public int OrderId { get; }
    public int CustomerId { get; }
    public List<string> Items { get; }
    public decimal Amount { get; set; }

    public Order(int orderId, int customerId, List<string> items)
    {
        OrderId = orderId;
        CustomerId = customerId;
        Items = items;
    }
}