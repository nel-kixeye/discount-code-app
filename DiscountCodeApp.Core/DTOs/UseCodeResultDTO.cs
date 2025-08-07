namespace DiscountCodeApp.Core.DTOs;
public enum UseCodeResultDTO
{
    Success = 0x00,
    Used = 0x01,
    InvalidLength = 0x02,
    NotFound = 0x03,
}
