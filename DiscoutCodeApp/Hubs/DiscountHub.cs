using DiscountCodeApp.Core.DTOs;
using DiscountCodeApp.Core.Helpers;
using DiscountCodeApp.Core.Interfaces;
using DiscountCodeApp.DTOs;
using DiscountCodeApp.Helpers;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace DiscountCodeApp.Hubs;

public class DiscountHub(IGenerateCodeService generateCodeService) : Hub
{
    private readonly IGenerateCodeService _generateCodeService = generateCodeService;

    public async Task GenerateCode(ushort count, byte length) 
    {
        try
        {
            var codes = await _generateCodeService.GenerateCodesAsync(count,length);
            var result = codes.Codes.Count > 0 ? true : false;
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
        if (string.IsNullOrWhiteSpace(code))
        {
            await Clients.Caller.SendAsync("ReceiveUseCodeResult", (byte)HubResultEnum.NullCode);
            return;
        }

        var trimmedCode = code.Trim();
        if (trimmedCode.Any(char.IsWhiteSpace))
        {
            await Clients.Caller.SendAsync("ReceiveUseCodeResult", (byte)HubResultEnum.WhitespaceDetected);
            return;
        }

        if (trimmedCode.Any(x => !Collections.charpool.Contains(x))) 
        {
            await Clients.Caller.SendAsync("ReceiveUseCodeResult", (byte)HubResultEnum.InvalidCharacter);
            return;
        }

        if (trimmedCode.Length < 7 || trimmedCode.Length > 8) 
        {
            await Clients.Caller.SendAsync("ReceiveUseCodeResult", (byte)HubResultEnum.InvalidLength);
            return;
        }

        try
        {
            var result = await _generateCodeService.UseCodeAsync(code);
            await Clients.Caller.SendAsync("ReceiveUseCodeResult", (byte)MapUseCodeResult(result));
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

    private static HubResultEnum MapUseCodeResult(UseCodeResultDTO useCodeResult) => useCodeResult switch
    {
        UseCodeResultDTO.Success => HubResultEnum.Success,
        UseCodeResultDTO.Used => HubResultEnum.Used,
        UseCodeResultDTO.NotFound => HubResultEnum.NotFound,
        _ => HubResultEnum.NotFound,
    };
    
}
