using DiscountCodeApp.Core.DTOs;
using DiscountCodeApp.DTOs;
using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Starting Discount Code Client...");

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5160/discount") // Update this to match your backend
    .WithAutomaticReconnect()
    .Build();

connection.On<bool>("ReceiveGeneratedCodes", result =>
{
    Console.WriteLine(result
        ? "Code generation succeeded."
        : "Code generation failed.");
});

connection.On<byte>("ReceiveUseCodeResult", result =>
{
    Console.WriteLine($"Use Code Result: {(HubResultEnum)result}");
});

try
{
    await connection.StartAsync();
    Console.WriteLine("Connected to SignalR Hub.");
}
catch (Exception ex)
{
    Console.WriteLine($"Connection failed: {ex.Message}");
    return;
}

while (true)
{
    Console.WriteLine("\nEnter command:");
    Console.WriteLine("[1] Generate codes");
    Console.WriteLine("[2] Use code");
    Console.WriteLine("[x] Exit");
    Console.Write("Choice: ");
    var input = Console.ReadLine()?.Trim();

    if (input == "1")
    {
        Console.Write("Enter count (1000–2000): ");
        if (!ushort.TryParse(Console.ReadLine(), out var count))
        {
            Console.WriteLine("Invalid count.");
            continue;
        }

        Console.Write("Enter code length (7 or 8): ");
        if (!byte.TryParse(Console.ReadLine(), out var length) || (length != 7 && length != 8))
        {
            Console.WriteLine("Invalid length. Must be 7 or 8.");
            continue;
        }

        try
        {
            await connection.InvokeAsync("GenerateCode", count, length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    else if (input == "2")
    {
        Console.Write("Enter code to use: ");
        var code = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(code))
        {
            Console.WriteLine("Code cannot be empty.");
            continue;
        }

        try
        {
            await connection.InvokeAsync("UseCode", code);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
    else if (input?.ToLower() == "x")
    {
        break;
    }
    else
    {
        Console.WriteLine("Invalid choice.");
    }
}

Console.WriteLine("Exiting...");
await connection.StopAsync();

