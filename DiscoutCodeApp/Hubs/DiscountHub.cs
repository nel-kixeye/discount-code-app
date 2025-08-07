using DiscountCodeApp.Core.Interfaces;
using DiscountCodeApp.Helpers;
using Microsoft.AspNetCore.SignalR;

namespace DiscountCodeApp.Hubs;

public class DiscountHub(IGenerateCodeService generateCodeService) : Hub
{
    private readonly IGenerateCodeService _generateCodeService = generateCodeService;

    public async Task GenerateCode(ushort count) 
    {
        try
        {
            var result = await _generateCodeService.GenerateCodesAsync(count);
            await Clients.Caller.SendAsync("ReceiveGeneratedCodes", result);
        }
        catch (ArgumentException ex)
        {
            throw HubExceptionHelpers.Wrap(ex, "Invalid count provided.");
        }
        catch (Exception ex)
        {
            throw HubExceptionHelpers.Wrap(ex, "Failed to generate discount codes.");
        }
    }

    public async Task UseCode(string code) 
    {
        try
        {
            if (string.IsNullOrWhiteSpace(code))
                HubExceptionHelpers.Throw("Code cannot be empty.");

            var result = await _generateCodeService.UseCodeAsync(code);
            await Clients.Caller.SendAsync("ReceiveUseCodeResult", result);
        }
        catch (KeyNotFoundException ex)
        {
            throw HubExceptionHelpers.Wrap(ex, "Code not found or already used.");
        }
        catch (Exception ex)
        {
            throw HubExceptionHelpers.Wrap(ex, "Failed to apply discount code.");
        }
    }
}
