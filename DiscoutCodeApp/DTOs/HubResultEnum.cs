namespace DiscountCodeApp.DTOs;

public enum HubResultEnum
{
    Success = 0x00,
    Used = 0x01,
    NotFound = 0x02,
    InvalidLength = 0x03,
    WhitespaceDetected = 0x04,
    NullCode = 0x05,
    InvalidCharacter = 0x06,
}
