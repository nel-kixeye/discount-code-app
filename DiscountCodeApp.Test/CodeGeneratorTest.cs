using DiscountCodeApp.Core.Interfaces;
using DiscountCodeApp.Infrastructure.Generator;

namespace DiscountCodeApp.Test;
public class CodeGeneratorTest
{
    public class CodeGeneratorTests
    {
        private readonly ICodeGenerator _generator;

        public CodeGeneratorTests()
        {
            _generator = new CodeGenerator();
        }

        [Theory]
        [InlineData(8)]
        [InlineData(16)]
        [InlineData(32)]
        public void Generate_ReturnsCode_OfCorrectLength(int length)
        {
            var code = _generator.Generate(length);

            Assert.Equal(length, code.Length);
        }

        [Fact]
        public void Generate_ReturnsDifferentCodes_OnMultipleCalls()
        {
            var code1 = _generator.Generate(12);
            var code2 = _generator.Generate(12);

            Assert.NotEqual(code1, code2);
        }

        [Fact]
        public void Generate_OnlyContainsValidCharacters()
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var length = 50;

            var code = _generator.Generate(length);

            Assert.All(code, c => Assert.Contains(c, validChars));
        }
    }
}
