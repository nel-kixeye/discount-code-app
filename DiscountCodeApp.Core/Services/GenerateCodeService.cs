using DiscountCodeApp.Core.DTOs;
using DiscountCodeApp.Core.Interfaces;
using DiscountCodeApp.Core.Models;

namespace DiscountCodeApp.Core.Services;
public class GenerateCodeService(
    IDiscountCodeRepository repository,
    ICodeGenerator generator) : IGenerateCodeService
{
    private readonly IDiscountCodeRepository _repository = repository;
    private readonly ICodeGenerator _generator = generator;

    public async Task<GenerateCodeResultDTO> GenerateCodesAsync(ushort count, byte length)
    {
        if (count < 1000 || count > 2000)
        {
            throw new ArgumentException("Invalid count range. Select from 1000 to 2000");
        }

        if (length < 7 || length > 8) 
        {
            throw new ArgumentException("Invalid length. Select from 7 to 8");
        }

        var existingCodes = await _repository.GetAllCodesAsync();
        var usedCodes = existingCodes
            .Select(x => x.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newCodes = new List<string>();
        var attemptBreaker = 0;

        while (newCodes.Count < count)
        {
            var candidateCode = _generator.Generate(length);

            if (!usedCodes.Contains(candidateCode) && !newCodes.Contains(candidateCode))
            {
                newCodes.Add(candidateCode);
            }

            if (++attemptBreaker > count * 10)
            {
                throw new Exception("Too many retries. Increase entropy size");
            }

        }

        var now = DateTime.UtcNow;
        var discountCodes = newCodes
            .Select(code => new DiscountCode
            {
                Code = code,
                IsUsed = false,
                CreatedAt = now,
                UpdatedAt = now,
            });

        await _repository.SaveCodesAsync(discountCodes);

        return new() { Codes = newCodes };
    }

    public async Task<UseCodeResultDTO> UseCodeAsync(string code) => await _repository.MarkCodeAsUsedAsync(code);
}
