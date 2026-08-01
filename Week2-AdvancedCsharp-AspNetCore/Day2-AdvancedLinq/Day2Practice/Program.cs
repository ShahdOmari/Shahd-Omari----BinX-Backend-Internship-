using System;
using System.Collections.Generic;
using System.Linq;

// Two related collections sharing a foreign key (CustomerId), so I can
// practice GroupBy, Join, and SelectMany the way they'd actually be used
// against relational-style data.

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

// Amount is just item count * 50 here, purely to have a number to
// group and sum by — not meant to reflect anything realistic.
foreach (var order in orders)
{
    order.Amount = order.Items.Count * 50;
}

// GroupBy clusters orders that share the same CustomerId into one group
// per customer, then Select() turns each group into a small summary
// object instead of leaving it as a raw group.
Console.WriteLine("----- Total Amount per Customer (GroupBy) -----");

var totalByCustomer = orders
    .GroupBy(o => o.CustomerId)
    .Select(g => new { CustomerId = g.Key, Total = g.Sum(o => o.Amount) });

foreach (var group in totalByCustomer)
{
    Console.WriteLine($"Customer {group.CustomerId}: Total = {group.Total}");
}

// Join matches customers to orders using c.Id == o.CustomerId, similar
// to a SQL inner join — customer 6 (Nour) has no orders, so she won't
// appear anywhere in this result at all.
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

// Each order has a list of items, so Select() alone would give me a
// list of lists (one items-list per order). SelectMany flattens that
// one level down into a single sequence of every item across every
// order, which is what I actually want here.
Console.WriteLine("\n----- All Items Across All Orders (SelectMany) -----");

var allItems = orders.SelectMany(o => o.Items);

foreach (var item in allItems)
{
    Console.WriteLine($"- {item}");
}

// Deferred execution demo: I'm defining the query here, but nothing
// runs yet. Where() just builds a plan — it doesn't touch 'orders'
// until something actually enumerates it (the foreach below).
Console.WriteLine("\n----- Deferred Execution Demo -----");

var highValueOrdersQuery = orders.Where(o => o.Amount >= 100);

// Modifying the source collection AFTER defining the query above,
// but BEFORE enumerating it below — this is the whole point of the demo.
orders.Add(new Order(107, 6, new List<string> { "Bookshelf", "Desk Lamp", "Rug" }));
orders[6].Amount = orders[6].Items.Count * 50;

Console.WriteLine("Query defined BEFORE the new order was added, but enumerated AFTER:");
foreach (var order in highValueOrdersQuery)
{
    Console.WriteLine($"- Order #{order.OrderId} (Amount: {order.Amount})");
}

Console.WriteLine("\nExplanation: Order #107 shows up here even though it didn't exist");
Console.WriteLine("yet when highValueOrdersQuery was defined. Where() doesn't capture a");
Console.WriteLine("snapshot of 'orders' at definition time — it re-checks the CURRENT");
Console.WriteLine("state of 'orders' at the moment the foreach loop actually runs. If I");
Console.WriteLine("needed the query to reflect the state of 'orders' at the point it was");
Console.WriteLine("defined instead, I'd need to call .ToList() right there to force it to");
Console.WriteLine("run immediately, before any further changes to the source.");


// ===== Type Declarations =====

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