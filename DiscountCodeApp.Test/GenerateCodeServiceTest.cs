using DiscountCodeApp.Core.DTOs;
using DiscountCodeApp.Core.Interfaces;
using DiscountCodeApp.Core.Models;
using DiscountCodeApp.Core.Services;
using Moq;

namespace DiscountCodeApp.Test;
public class GenerateCodeServiceTest
{
    private readonly Mock<IDiscountCodeRepository> _repoMock = new();
    private readonly Mock<ICodeGenerator> _generatorMock = new();
    private readonly GenerateCodeService _service;

    public GenerateCodeServiceTest()
    {
        _service = new GenerateCodeService(
            _repoMock.Object, 
            _generatorMock.Object);
    }

    [Fact]
    public async Task GenerateCodesAsync_ShouldGenerate1000UniqueCodes()
    {
        var count = 1000;
        var fakeCodes = Enumerable.Range(0, count)
                                   .Select(i => $"CODE{i:D4}")
                                   .ToArray();
        var index = 0;

        _repoMock.Setup(r => r.GetAllCodesAsync())
                 .ReturnsAsync(new List<DiscountCode>());

        _generatorMock.Setup(g => g.Generate(8))
                      .Returns(() => fakeCodes[index++]);

        var result = await _service.GenerateCodesAsync((ushort)count);

        Assert.Equal(count, result.Codes.Count());
        Assert.All(result.Codes, code => Assert.Contains(code, fakeCodes));
        _repoMock.Verify(r => r.SaveCodesAsync(It.IsAny<IEnumerable<DiscountCode>>()), Times.Once);
    }

    [Fact]
    public async Task GenerateCodesAsync_ShouldThrowIfCountIsTooLow()
    {
        var lowCount = 3;

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GenerateCodesAsync((ushort)lowCount));
    }

    [Fact]
    public async Task GenerateCodesAsync_ShouldThrowIfCountExcedes()
    {
        var lowCount = 4000;

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.GenerateCodesAsync((ushort)lowCount));
    }
}
