namespace DiscountCodeApp.Core.Models;
public class DiscountCode
{
    public string Code { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
