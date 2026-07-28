// Value Types Example 
int x = 20;
int y = x;
y = 60;

Console.WriteLine($"x = {x}"); 
Console.WriteLine($"y = {y}"); 

// Reference Types Example 
int[] arr1 = { 1, 2, 3 };
int[] arr2 = arr1;
arr2[0] = 99;

Console.WriteLine($"arr1[0] = {arr1[0]}"); 
Console.WriteLine($"arr2[0] = {arr2[0]}");   

// String Example (special case) 
string name1 = "Shahd";
string name2 = name1;
name2 = "Omari";

Console.WriteLine($"name1 = {name1}"); 
Console.WriteLine($"name2 = {name2}");  

//  Switch Expression Example 
int age = 17;

string ageCategory = age switch
{
    < 13 => "Child",
    < 20 => "Teenager",
    < 60 => "Adult",
    _    => "Senior"
};

Console.WriteLine($"Age {age} is classified as: {ageCategory}"); 

//  For Loop Example 
int number = 5;
Console.WriteLine($"Multiplication table for {number}:");
for (int i = 1; i <= 5; i++)
{
    Console.WriteLine($"{number} x {i} = {number * i}"); 

} 

// Foreach Loop Example 
string[] cities = { "Nablus", "Ramallah", "Hebron", "Jenin" };

Console.WriteLine("Cities in Palestine:");
foreach (var city in cities)
{
    Console.WriteLine($"- {city}");
} 

//  While Loop Example 
int countdown = 5;

Console.WriteLine("Countdown:");
while (countdown > 0)
{
    Console.WriteLine(countdown);
    countdown--;
}
Console.WriteLine("Go!");  

//  Nullable Reference Type Example 
string? email = null;

if (email != null)
{
    Console.WriteLine($"Email length: {email.Length}");
}
else
{
    Console.WriteLine("No email provided.");
}


email = "shahd@example.com";
if (email != null)
{
    Console.WriteLine($"Email length: {email.Length}");
} 

