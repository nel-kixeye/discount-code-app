using DiscountCodeApp.Core.DTOs;
using DiscountCodeApp.Core.Models;

namespace DiscountCodeApp.Core.Interfaces;
public interface IDiscountCodeRepository
{
    Task<IEnumerable<DiscountCode>> GetAllCodesAsync();
    Task SaveCodesAsync(IEnumerable<DiscountCode> codes);
    Task<UseCodeResultDTO> MarkCodeAsUsedAsync(string code);
}
