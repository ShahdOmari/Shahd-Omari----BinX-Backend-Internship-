# Day 2 — C# Fundamentals I: Types, Variables & Control Flow

**8 hours**

## Learning Objectives

- Distinguish value types from reference types and explain why the
  difference matters
- Use variables, type inference, and clear naming conventions
  idiomatically
- Write control flow using if statements, switch expressions, and
  loops

## What I Did

- Wrote value-type examples (`int`) and reference-type examples
  (`int[]`, `string`) to directly observe copy-by-value vs.
  copy-by-reference behavior
- Practiced switch expressions for classification logic (age
  categories, grade levels)
- Wrote `for`, `foreach`, and `while` loops for different scenarios
  (multiplication table, iterating a list, countdown)
- Practiced nullable reference types (`string?`) and handled a
  possibly-null value safely with an `if` check instead of the `!`
  operator

## Key Code Examples

```csharp
// Value type: independent copy
int x = 20;
int y = x;
y = 60;
// x is still 20

// Reference type: shared reference
int[] arr1 = { 1, 2, 3 };
int[] arr2 = arr1;
arr2[0] = 99;
// arr1[0] is now 99 too
```

## What I Learned

The value vs. reference type distinction is the one thing I'll need
to keep in mind constantly going forward. Testing it directly made it
concrete: mutating a reference type (like an array) through one
variable affected every other variable pointing to it, while a value
type copy stayed completely independent. Strings behave like
reference types but *act* immutable, which was the trickiest part to
internalize — reassigning a string doesn't mutate the original, it
points the variable at a brand new string entirely.

## Project

[`Day2Practice/`](Day2Practice/) — value/reference type demos, switch
expressions, loops, and nullable reference type handling.