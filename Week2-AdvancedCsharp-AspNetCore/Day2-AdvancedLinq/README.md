# Day 2 — Advanced LINQ & Deferred Execution

**8 hours**

## Learning Objectives

- Explain the difference between deferred and immediate LINQ
  execution
- Use GroupBy and Join to reshape and combine data
- Flatten nested collections using SelectMany

## What I Did

- Created two related collections (`Customers` and `Orders`, 6 items
  each) sharing a `CustomerId` foreign key
- Wrote a `GroupBy` query summarizing the total order amount per
  customer
- Wrote a `Join` query combining customer names with their order
  amounts
- Wrote a `SelectMany` query flattening every item across every order
  into a single sequence
- Demonstrated deferred execution directly: defined a query with
  `.Where()`, added a new order to the source list *after* defining
  the query but *before* enumerating it, then showed the new order
  appeared in the results anyway

## Key Code Example

```csharp
var totalByCustomer = orders
    .GroupBy(o => o.CustomerId)
    .Select(g => new { CustomerId = g.Key, Total = g.Sum(o => o.Amount) });

var customerOrderDetails = customers
    .Join(orders, c => c.Id, o => o.CustomerId,
          (c, o) => new { c.Name, o.OrderId, o.Amount });

var allItems = orders.SelectMany(o => o.Items);
```

## What I Learned

Deferred execution stopped being an abstract warning and became
something I could actually see happen — a new order (#107) showed up
in query results even though it didn't exist yet when the query was
defined. `Where()` doesn't capture a snapshot of the source collection
at definition time — it re-checks the current state of the collection
at the moment it's actually enumerated. If I needed the query to
reflect the state at the point it was defined instead, calling
`.ToList()` right there would force it to run immediately.

## Project

[`Day2Practice/`](Day2Practice/) — GroupBy, Join, SelectMany, and a
live deferred execution demo.