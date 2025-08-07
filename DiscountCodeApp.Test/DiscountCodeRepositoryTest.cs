using DiscountCodeApp.Core.Models;
using DiscountCodeApp.Infrastructure.Repository;

namespace DiscountCodeApp.Test;
public class DiscountCodeRepositoryTest : IDisposable
{
    private readonly string _tempFile;
    private readonly DiscountCodeRepository _repository;

    public DiscountCodeRepositoryTest()
    {
        _tempFile = Path.GetTempFileName(); 
        _repository = new DiscountCodeRepository(_tempFile);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }

    [Fact]
    public async Task GetAllCodesAsync_ShouldReturnEmpty_WhenFileIsEmpty()
    {
        await File.WriteAllTextAsync(_tempFile, "[]");

        var codes = await _repository.GetAllCodesAsync();

        Assert.NotNull(codes);
        Assert.Empty(codes);
    }

    [Fact]
    public async Task SaveCodesAsync_ShouldWriteToFile_AndReturnWithGetAll()
    {
        var codes = new List<DiscountCode>
            {
                new DiscountCode { Code = "TEST123", CreatedAt = DateTime.UtcNow, IsUsed = false },
                new DiscountCode { Code = "HELLO456", CreatedAt = DateTime.UtcNow, IsUsed = false }
            };

        await _repository.SaveCodesAsync(codes);

        var saved = await _repository.GetAllCodesAsync();

        Assert.Equal(2, saved.Count());
        Assert.Contains(saved, c => c.Code == "TEST123");
    }

    [Fact]
    public async Task MarkCodeAsUsedAsync_ShouldMarkAndReturnTrue()
    {
        var now = DateTime.UtcNow;
        var codes = new List<DiscountCode>
            {
                new DiscountCode { Code = "USED999", CreatedAt = now, IsUsed = false }
            };

        await _repository.SaveCodesAsync(codes);

        var result = await _repository.MarkCodeAsUsedAsync("USED999");

        Assert.True(result);

        var allCodes = (await _repository.GetAllCodesAsync()).ToList();
        Assert.Single(allCodes);
        Assert.True(allCodes[0].IsUsed);
        Assert.True(allCodes[0].UpdatedAt > now);
    }

    [Fact]
    public async Task MarkCodeAsUsedAsync_ShouldReturnFalse_WhenCodeNotFound()
    {
        var result = await _repository.MarkCodeAsUsedAsync("INVALID");
        Assert.False(result);
    }

    [Fact]
    public async Task SaveCodesAsync_ShouldAppendAndNotOverwriteExistingCodes()
    {
        var tempFilePath = Path.GetTempFileName();

        var repo = new DiscountCodeRepository(tempFilePath);

        var now = DateTime.UtcNow;

        var firstBatch = Enumerable.Range(0, 1000)
            .Select(i => new DiscountCode
            {
                Code = $"FIRST{i:D4}",
                IsUsed = false,
                CreatedAt = now,
                UpdatedAt = now
            });

        var secondBatch = Enumerable.Range(0, 1000)
            .Select(i => new DiscountCode
            {
                Code = $"SECOND{i:D4}",
                IsUsed = false,
                CreatedAt = now,
                UpdatedAt = now
            });

        await repo.SaveCodesAsync(firstBatch);
        await repo.SaveCodesAsync(secondBatch);

        var allCodes = await repo.GetAllCodesAsync();

        Assert.Equal(2000, allCodes.Count());
        Assert.Equal(2000, allCodes.Select(c => c.Code).Distinct().Count());

        File.Delete(tempFilePath);
    }

    [Fact]
    public async Task SaveCodesUnsafeAsync_Should_Not_Save_Duplicate_Codes()
    {
        var tempFilePath = Path.GetTempFileName();
        var repository = new DiscountCodeRepository(tempFilePath);

        var code = new DiscountCode { Code = "DUPLICATE123", IsUsed = false };
        var codes = Enumerable.Repeat(code, 1000).ToList();

        await repository.SaveCodesAsync(codes);

        await repository.SaveCodesAsync(codes);

        var allCodes = await repository.GetAllCodesAsync();

        var distinctCodesCount = allCodes.Select(c => c.Code).Distinct().Count();
        var totalCodesCount = allCodes.ToList().Count;

        Assert.Equal(distinctCodesCount, totalCodesCount);
    }

    [Fact]
    public async Task SaveCodesAsync_OverwritesDuplicatesWithLatest()
    {
        var tempFilePath = Path.GetTempFileName();
        var repository = new DiscountCodeRepository(tempFilePath);

        var code = "DUPLICATE-CODE";

        var firstVersion = new DiscountCode { Code = code, IsUsed = false };
        var secondVersion = new DiscountCode { Code = code, IsUsed = true };

        await repository.SaveCodesAsync(new List<DiscountCode> { firstVersion });
        await repository.SaveCodesAsync(new List<DiscountCode> { secondVersion });

        var allCodes = (await repository.GetAllCodesAsync()).ToList();

        Assert.Single(allCodes); 
        Assert.Equal(code, allCodes[0].Code);
        Assert.True(allCodes[0].IsUsed); 
    }
}
