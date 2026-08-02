using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// Simulating 3 independent "data sources" with different delays, so I can
// actually see a measurable difference between running them one after
// another vs. running them concurrently.

var stopwatch = new Stopwatch();

// ----- 1. Sequential calls (individual awaits) -----
Console.WriteLine("----- Sequential Execution -----");

stopwatch.Start();

string result1 = await FetchFromDatabaseAsync();
string result2 = await FetchFromExternalApiAsync();
string result3 = await FetchFromCacheAsync();

stopwatch.Stop();

Console.WriteLine(result1);
Console.WriteLine(result2);
Console.WriteLine(result3);
Console.WriteLine($"Sequential total time: {stopwatch.ElapsedMilliseconds} ms");

// Each await here waits for the previous one to fully finish before even
// starting the next call, even though none of these 3 sources depend on
// each other. That's why the total ends up being roughly the SUM of all
// three delays (2000 + 1500 + 1000 = ~4500ms).


// ----- 2. Concurrent calls using Task.WhenAll -----
Console.WriteLine("\n----- Concurrent Execution (Task.WhenAll) -----");

stopwatch.Restart();

// Calling the methods WITHOUT awaiting yet — this starts all 3 tasks
// running at (roughly) the same time, instead of one after another.
Task<string> taskA = FetchFromDatabaseAsync();
Task<string> taskB = FetchFromExternalApiAsync();
Task<string> taskC = FetchFromCacheAsync();

// Now I wait for all 3 to finish together.
await Task.WhenAll(taskA, taskB, taskC);

stopwatch.Stop();

Console.WriteLine(taskA.Result);
Console.WriteLine(taskB.Result);
Console.WriteLine(taskC.Result);
Console.WriteLine($"Concurrent total time: {stopwatch.ElapsedMilliseconds} ms");

// This time the total is roughly the SLOWEST single delay (~2000ms),
// not the sum of all three. Note: I'm using .Result here only because
// Task.WhenAll already guarantees the tasks are complete at this point,
// so it's safe — this is different from calling .Result on a task that
// might still be running, which is the blocking mistake from the lesson.


// ----- 3. Demonstrating cancellation mid-operation -----
Console.WriteLine("\n----- Cancellation Demo -----");

using var cts = new CancellationTokenSource();
cts.CancelAfter(2500); // simulate the caller cancelling partway through

try
{
    string longResult = await FetchLargeReportAsync(cts.Token);
    Console.WriteLine(longResult);
}
catch (OperationCanceledException)
{
    Console.WriteLine("FetchLargeReportAsync was cancelled before completing.");
}


// ===== Local async methods simulating different data sources =====

// Represents something slow, like a real database query.
static async Task<string> FetchFromDatabaseAsync()
{
    await Task.Delay(2000);
    return "Database: data loaded";
}

// Represents a call to a third-party API over the network.
static async Task<string> FetchFromExternalApiAsync()
{
    await Task.Delay(1500);
    return "External API: response received";
}

// Represents an in-memory cache lookup, still async because in a real
// app this might be a distributed cache over the network (like Redis),
// not guaranteed to be instant.
static async Task<string> FetchFromCacheAsync()
{
    await Task.Delay(1000);
    return "Cache: value retrieved";
}

// A longer operation broken into steps, checking the cancellation token
// between each one. This is what makes cancellation actually work mid-way
// through — if I only checked the token once at the start, cancelling
// after step 2 wouldn't do anything useful.
static async Task<string> FetchLargeReportAsync(CancellationToken cancellationToken)
{
    for (int step = 1; step <= 5; step++)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Console.WriteLine($"Generating report... step {step}/5");

        // Passing the token into Task.Delay too, so the wait itself gets
        // interrupted immediately instead of finishing the full delay
        // before the next check even runs.
        await Task.Delay(1000, cancellationToken);
    }

    return "Report generation complete.";
}