using DiscountCodeApp.Core.Helpers;
using DiscountCodeApp.Core.Interfaces;
using System.Security.Cryptography;

namespace DiscountCodeApp.Infrastructure.Generator;
public class CodeGenerator : ICodeGenerator
{
    public string Generate(int length)
    {
        var code = new char[length];
        var buffer = new byte[4];

        using var rng = RandomNumberGenerator.Create();

        for (int i = 0; i < length; i++)
        {
            rng.GetBytes(buffer);
            uint num = BitConverter.ToUInt32(buffer, 0);
            code[i] = Collections.charpool[num % Collections.charpool.Length];
        }

        return new string(code);

    }
}
