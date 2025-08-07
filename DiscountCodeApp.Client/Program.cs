using DiscountCodeApp.Core.DTOs;
using Microsoft.AspNetCore.SignalR.Client;

Console.WriteLine("Starting Discount Code Client...");

// Connect to the hub
var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5160/discount") // Change this to your backend URL
    .WithAutomaticReconnect()
    .Build();

// Receive handler for generated codes
connection.On<GenerateCodeResultDTO>("ReceiveGeneratedCodes", codes =>
{
    var unpacked = codes.Codes;
    Console.WriteLine("Generated Codes:");
    foreach (var code in unpacked)
        Console.WriteLine($"- {code}");
});

// Receive handler for use code result
connection.On<UseCodeResultDTO>("ReceiveUseCodeResult", result =>
{
    Console.WriteLine($"Use Code Result: {result.Result}");
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

// Command loop
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
        Console.Write("Enter count: ");
        if (ushort.TryParse(Console.ReadLine(), out var count))
        {
            try
            {
                await connection.InvokeAsync("GenerateCode", count);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        else
        {
            Console.WriteLine("Invalid count.");
        }
    }
    else if (input == "2")
    {
        Console.Write("Enter code to use: ");
        var code = Console.ReadLine();
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
