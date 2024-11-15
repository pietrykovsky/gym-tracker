using FluentAssertions;
using GymTracker.Data;
using GymTracker.Services;
using System.Threading.Tasks;

namespace GymTracker.Tests.Services;

public class TrainingPlanCategoryServiceTests
{
    private readonly TrainingPlanCategoryService _sut;
    private readonly TestApplicationDbContext _dbContext;

    public TrainingPlanCategoryServiceTests()
    {
        var factory = new DbContextFactoryWrapper();
        _dbContext = (TestApplicationDbContext)factory.CreateDbContext();
        _sut = new TrainingPlanCategoryService(factory);
        _dbContext.Database.EnsureDeleted();

        SeedData();
    }

    private void SeedData()
    {
        var categories = new[]
        {
            new TrainingPlanCategory { Id = 1, Name = "Strength", Description = "Strength training plans" },
            new TrainingPlanCategory { Id = 2, Name = "Cardio", Description = "Cardio training plans" },
            new TrainingPlanCategory { Id = 3, Name = "Flexibility", Description = "Flexibility training plans" }
        };

        _dbContext.TrainingPlanCategories.AddRange(categories);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsAllCategories()
    {
        // Act
        var result = await _sut.GetAllCategoriesAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(c => c.Name);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ReturnsCorrectCategory()
    {
        // Act
        var result = await _sut.GetCategoryByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Strength");
        result.Description.Should().Be("Strength training plans");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _sut.GetCategoryByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllCategoriesAsync_WithNoCategories_ReturnsEmptyList()
    {
        // Arrange
        _dbContext.TrainingPlanCategories.RemoveRange(_dbContext.TrainingPlanCategories);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _sut.GetAllCategoriesAsync();

        // Assert
        result.Should().BeEmpty();
    }
}