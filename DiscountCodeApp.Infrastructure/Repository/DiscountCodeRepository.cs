using DiscountCodeApp.Core.DTOs;
using DiscountCodeApp.Core.Interfaces;
using DiscountCodeApp.Core.Models;
using System.Text.Json;

namespace DiscountCodeApp.Infrastructure.Repository;
public class DiscountCodeRepository : IDiscountCodeRepository
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public DiscountCodeRepository(string? filePath = null)
    {
        _filePath = filePath ?? "discount_codes.json";
    }

    public async Task<IEnumerable<DiscountCode>> GetAllCodesAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return await ReadAllCodesUnsafeAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task SaveCodesAsync(IEnumerable<DiscountCode> codes)
    {
        await _lock.WaitAsync();
        try
        {
            await SaveCodesUnsafeAsync([.. codes]);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<UseCodeResultDTO> MarkCodeAsUsedAsync(string code)
    {
        await _lock.WaitAsync();
        try
        {
            var allCodes = await ReadAllCodesUnsafeAsync();

            var list = allCodes.ToList();
            var target = list.FirstOrDefault(c => c.Code == code);

            if (target == null) 
            {
                return UseCodeResultDTO.NotFound;
            }

            if (target.IsUsed) 
            {
                return UseCodeResultDTO.Used;
            }

            target.IsUsed = true;
            target.UpdatedAt = DateTime.UtcNow;

            await SaveCodesUnsafeAsync(list);
            return UseCodeResultDTO.Success;
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<DiscountCode>> ReadAllCodesUnsafeAsync()
    {
        if (!File.Exists(_filePath))
        {
            await File.WriteAllTextAsync(_filePath, "[]");
            return [];
        }

        var json = await File.ReadAllTextAsync(_filePath);
        if (string.IsNullOrEmpty(json))
        {
            return [];
        }
        var codes = JsonSerializer.Deserialize<List<DiscountCode>>(json);
        return codes ?? [];
    }

    private async Task SaveCodesUnsafeAsync(List<DiscountCode> codes)
    {
        var existingCodes = await ReadAllCodesUnsafeAsync();

        var merged = existingCodes
            .Concat(codes)
            .GroupBy(c => c.Code)
            .Select(g => g.Last())
            .ToList();

        var json = JsonSerializer.Serialize(merged);
        await File.WriteAllTextAsync(_filePath, json);
    }
}
