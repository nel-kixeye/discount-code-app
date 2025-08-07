using DiscountCodeApp.Core.DTOs;

namespace DiscountCodeApp.Core.Interfaces;
public interface IGenerateCodeService
{
    Task<GenerateCodeResultDTO> GenerateCodesAsync(ushort count);
    Task<UseCodeResultDTO> UseCodeAsync(string code);
}
