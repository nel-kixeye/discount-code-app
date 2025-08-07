using Microsoft.AspNetCore.SignalR;

namespace DiscountCodeApp.Helpers;

public class HubExceptionHelpers
{
    public static void Throw(string message)
    {
        throw new HubException(message);
    }

    public static HubException Wrap(Exception ex, string? userMessage = null)
    {
        return new HubException(userMessage ?? "An error occurred.", ex);
    }
}
